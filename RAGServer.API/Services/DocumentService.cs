

using RAGSERVERAPI.DTOs;
using RAGSERVERAPI.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Packaging;
using System.Text;
using Dapper;

namespace RAGSERVERAPI.Services;

public interface IDocumentService
{
    Task<DocumentUploadResponse> UploadDocumentAsync(UploadDocumentRequest request, Guid userId);
    Task<List<DocumentListResponse>> GetUserDocumentsAsync(Guid userId, Guid? tenantId = null);
    Task<Document?> GetDocumentByIdAsync(Guid documentId);
    Task<bool> DeleteDocumentAsync(Guid documentId, Guid userId);
    Task<object> ProcessDocumentAsync(Guid documentId);
    Task<List<SearchResults>> SearchByEmbeddingAsync(float[] queryEmbedding, SearchRequest request);
}
public class DocumentService : IDocumentService
{
    private readonly IGcsService _gcsService;
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentChunkRepository _chunkRepository;
    private readonly IEmbeddingRepository _embeddingRepository;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger _logger;
    private readonly IAdobeService _adobeService;
    private readonly DataContext _context;
    private readonly IConfigurationService _configurationService;

    public DocumentService(IGcsService gcsService, IDocumentRepository documentRepository, IDocumentChunkRepository chunkRepository,
    IEmbeddingRepository embeddingRepository, IEmbeddingService embeddingService, ILogger logger, IAdobeService adobeService, DataContext context, IConfigurationService configurationService)
    {
        _gcsService = gcsService;
        _documentRepository = documentRepository;
        _chunkRepository = chunkRepository;
        _embeddingService = embeddingService;
        _logger = logger;
        _adobeService = adobeService;
        _context = context;
        _embeddingRepository = embeddingRepository;
        _configurationService = configurationService;
    }

    public async Task<DocumentUploadResponse> UploadDocumentAsync(UploadDocumentRequest request, Guid userId)
    {
        try
        {
            _logger.LogInfo($"Uploading document {request.FileName} for user {userId}");

            // Upload to GCS
            var gcsUri = await _gcsService.UploadFileAsync(
                request.FileName,
                request.FileContent,
                request.ContentType,
                request.IsPublic
            );

            // Create document record
            var document = new Document
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                RoleId = request.RoleId,
                UserId = userId,
                FileName = Guid.NewGuid().ToString() + Path.GetExtension(request.FileName),
                OriginalFileName = request.FileName,
                BucketPath = gcsUri.Replace($"gs://", ""),
                ContentType = request.ContentType,
                FileSize = request.FileContent.Length,
                GcsUri = gcsUri,
                IsPublic = request.IsPublic,
                Status = DocumentStatus.Uploading.ToString().ToLower(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _documentRepository.CreateAsync(document);

            _logger.LogInfo($"Document {document.Id} uploaded successfully");

            // Process document asynchronously (in real production, use a background job)
            //_ = Task.Run(async () => await ProcessDocumentAsync(document.Id));

            return new DocumentUploadResponse(
                DocumentId: document.Id,
                FileName: document.OriginalFileName,
                GcsUri: document.GcsUri,
                Status: document.Status
            );
        }
        catch (Exception ex)
        {
            _logger.LogLocationWithException($"DocumentService:UploadDocumentAsync Error uploading document {request.FileName}", ex);
            throw;
        }
    }

    public async Task<List<DocumentListResponse>> GetUserDocumentsAsync(Guid userId, Guid? tenantId = null)
    {
        try
        {
            var documents = await _documentRepository.GetByUserIdAsync(userId, tenantId);

            return documents.Select(d => new DocumentListResponse(
                Id: d.Id,
                FileName: d.OriginalFileName,
                ContentType: d.ContentType,
                FileSize: d.FileSize,
                Status: d.Status,
                CreatedAt: d.CreatedAt
            )).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogLocationWithException($"DocumentService:GetUserDocumentsAsync: Error getting documents for user {userId}", ex);
            throw;
        }
    }

    public async Task<Document?> GetDocumentByIdAsync(Guid documentId)
    {
        return await _documentRepository.GetByIdAsync(documentId);
    }

    public async Task<bool> DeleteDocumentAsync(Guid documentId, Guid userId)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null || document.UserId != userId)
            {
                return false;
            }

            await _gcsService.DeleteFileAsync(document.GcsUri);
            await _documentRepository.DeleteAsync(documentId);

            _logger.LogInfo($"Document {documentId} deleted successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogLocationWithException($"DocumentService:DeleteDocumentAsync: Error deleting document {documentId}", ex);
            throw;
        }
    }

    public async Task<object> ProcessDocumentAsync(Guid documentId)
    {
        try
        {

            _logger.LogInfo($"Processing document {documentId}");
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
            {
                _logger.LogInfo($"Document {documentId} not found");
                return null;
            }
            await _documentRepository.UpdateStatusAsync(documentId, DocumentStatus.Processing.ToString().ToLower());
            await _embeddingRepository.DeleteDocumentEmbeddings(documentId);
            await _chunkRepository.DeleteDocumentChuncks(documentId);
            // Download file from GCS
            var fileContent = await _gcsService.DownloadFileAsync(document.GcsUri);

            // Extract text based on content type
            string extractedText = document.ContentType switch
            {
                "application/pdf" => ExtractTextFromPdf(fileContent),
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ExtractTextFromDocx(fileContent),
                "text/plain" => Encoding.UTF8.GetString(fileContent),
                _ => throw new NotSupportedException($"Content type {document.ContentType} is not supported")
            };

            if (string.IsNullOrEmpty(extractedText))
            {
                if (document.ContentType == "application/pdf")
                {
                    var tempDir = Path.GetTempPath();
                    var FileName = $"{Guid.NewGuid()}-{document.FileName}";
                    var tempFilePath = Path.Combine(tempDir, FileName);
                    var uploadPdfResponse = await _adobeService.UploadPdf(
                        tempFilePath,
                        FileName,
                        tempDir
                    );
                    if (uploadPdfResponse.IsRepoError == false)
                    {
                        extractedText = uploadPdfResponse.Data.PdfText.Replace("\\n", string.Empty);
                    }
                }
            }
            // Split text into chunks
            var config = await _configurationService.GetRagConfigurationAsync();
            if (extractedText.Length > config.MaxCharsPerInstance * 10)
            {
                _logger.LogInfo($"Document very large: {extractedText.Length} chars");
            }
            var chunks = SplitTextIntoChunks(extractedText, config.ChunkSize, config.ChunkOverlap);
            _logger.LogInfo($"Split document {documentId} into {chunks.Count} chunks");

            // Save chunks and generate embeddings
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = new DocumentChunk
                {
                    Id = Guid.NewGuid(),
                    DocumentId = documentId,
                    ChunkIndex = i,
                    Content = chunks[i],
                    TokenCount = EstimateTokenCount(chunks[i]),
                    CreatedAt = DateTime.UtcNow
                };

                await _chunkRepository.CreateAsync(chunk);

                // Generate and store embedding
                await _embeddingService.CreateEmbeddingAsync(
                    chunk.Id,
                    documentId,
                    document.TenantId,
                    document.RoleId,
                    chunk.Content
                );

                _logger.LogInfo($"Created embedding for chunk {i} of document {documentId}");
            }
            // Update document status
            await _documentRepository.UpdateStatusAsync(documentId, DocumentStatus.Indexed.ToString().ToLower());
            _logger.LogInfo($"Successfully processed document {documentId}");
            return new
            {
                documentId,
                chunkCount = chunks.Count,
                status = DocumentStatus.Indexed.ToString().ToLower(),
            };
        }
        catch (Exception ex)
        {
            _logger.LogLocationWithException($"DocumentService:ProcessDocumentAsync: Error processing document {documentId}", ex);
            await _documentRepository.UpdateStatusAsync(documentId, DocumentStatus.Error.ToString().ToLower());
            throw;
        }
    }

    public async Task<List<SearchResults>> SearchByEmbeddingAsync(float[] queryEmbedding, SearchRequest request)
    {
        try
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            // Convert float array to PostgreSQL vector format
            var vectorString = $"[{string.Join(",", queryEmbedding)}]";

            // Optimized query with better join strategy and index hints
            var sql = @"
                    WITH ranked_embeddings AS (
                        SELECT 
                            e.documentid,
                            e.chunkid,
                            1 - (e.embedding <=> @QueryEmbedding::vector) as similarityscore
                        FROM embeddings e
                        WHERE 1=1";

            var parameters = new DynamicParameters();
            parameters.Add("@QueryEmbedding", vectorString);
            parameters.Add("@TopK", request.TopK);
            parameters.Add("@SimilarityThreshold", request.SimilarityThreshold);

            // Add tenant filter if provided
            if (request.TenantId.HasValue)
            {
                sql += " AND e.tenantid = @TenantId";
                parameters.Add("@TenantId", request.TenantId.Value);
            }

            // Add role filter if provided
            if (request.RoleId.HasValue)
            {
                sql += " AND e.roleid = @RoleId";
                parameters.Add("@RoleId", request.RoleId.Value);
            }

            // Complete the CTE with similarity threshold and limit
            sql += @"
                            AND (1 - (e.embedding <=> @QueryEmbedding::vector)) >= @SimilarityThreshold
                        ORDER BY e.embedding <=> @QueryEmbedding::vector
                        LIMIT @TopK
                    )
                    SELECT 
                        re.documentid,
                        re.chunkid,
                        dc.content,
                        d.filename,
                        d.originalfilename,
                        dc.chunkindex,
                        d.metadata::text as documentmetadata,
                        dc.metadata::text as chunkmetadata,
                        re.similarityscore
                    FROM ranked_embeddings re
                    INNER JOIN documentchunks dc ON re.chunkid = dc.id
                    INNER JOIN documents d ON re.documentid = d.id
                    ORDER BY re.similarityscore DESC";

            var results = await connection.QueryAsync<SearchResultDto>(sql, parameters, commandTimeout: 30);

            return results.Select(r => new SearchResults
            {
                DocumentId = r.DocumentId,
                ChunkId = r.ChunkId,
                Content = r.Content,
                FileName = r.FileName,
                OriginalFileName = r.OriginalFileName,
                SimilarityScore = r.SimilarityScore,
                ChunkIndex = r.ChunkIndex,
                DocumentMetadata = !string.IsNullOrEmpty(r.DocumentMetadata)
                    ? System.Text.Json.JsonDocument.Parse(r.DocumentMetadata)
                    : null,
                ChunkMetadata = !string.IsNullOrEmpty(r.ChunkMetadata)
                    ? System.Text.Json.JsonDocument.Parse(r.ChunkMetadata)
                    : null
            }).ToList();
        }
        catch (Exception ex)
        {
            // _logger.LogError(ex, "Error searching embeddings");
            throw;
        }
    }

    #region private method
    private string ExtractTextFromPdf(byte[] pdfContent)
    {
        using var stream = new MemoryStream(pdfContent);
        using var document = PdfDocument.Open(stream);

        var text = new StringBuilder();
        foreach (var page in document.GetPages())
        {
            text.AppendLine(page.Text);
        }

        return text.ToString();
    }

    private string ExtractTextFromDocx(byte[] docxContent)
    {
        using var stream = new MemoryStream(docxContent);
        using var wordDoc = WordprocessingDocument.Open(stream, false);

        var body = wordDoc.MainDocumentPart?.Document.Body;
        if (body == null) return string.Empty;

        return body.InnerText;
    }

    private List<string> SplitTextIntoChunks(string text, int chunkSize, int chunkOverlap)
    {
        var chunks = new List<string>();
        var currentPosition = 0;

        while (currentPosition < text.Length)
        {
            var size = Math.Min(chunkSize, text.Length - currentPosition);
            var chunk = text.Substring(currentPosition, size);

            // Try to end chunk at a sentence boundary
            if (currentPosition + size < text.Length)
            {
                var lastPeriod = chunk.LastIndexOf('.');
                var lastNewline = chunk.LastIndexOf('\n');
                var lastBoundary = Math.Max(lastPeriod, lastNewline);

                if (lastBoundary > chunkSize / 2)
                {
                    chunk = chunk.Substring(0, lastBoundary + 1);
                    currentPosition += lastBoundary + 1;
                }
                else
                {
                    currentPosition += size;
                }
            }
            else
            {
                currentPosition += size;
            }

            chunks.Add(chunk.Trim());

            // Apply overlap
            if (currentPosition < text.Length)
            {
                currentPosition -= chunkOverlap;
                if (currentPosition < 0) currentPosition = 0;
            }
        }

        return chunks;
    }

    private int EstimateTokenCount(string text)
    {
        // Rough estimation: 1 token ≈ 4 characters
        return text.Length / 4;
    }
    #endregion
}
