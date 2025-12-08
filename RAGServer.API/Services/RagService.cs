
using RAGSERVERAPI.DTOs;
using RAGSERVERAPI.Models;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text;
namespace RAGSERVERAPI.Services;

public interface IRagService
{
    Task<ChatResponse> GenerateResponseAsync(ChatRequest request, Guid userId);
    IAsyncEnumerable<string> GenerateResponseStreamAsync(StreamChatRequest request, Guid userId);
    Task<List<SearchResult>> SearchDocumentsAsync(SearchDocumentsRequest request);
}
public class RagService : IRagService
{
    private readonly IVertexAIService _vertexAIService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IDocumentChunkRepository _chunkRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IConversationService _conversationService;
    private readonly ILogger _logger;
    private readonly IConfigurationService _configurationService;

    public RagService(IVertexAIService vertexAIService, IEmbeddingService embeddingService, IDocumentChunkRepository chunkRepository,
        IDocumentRepository documentRepository, IConversationService conversationService, ILogger logger, IConfigurationService configurationService)
    {
        _vertexAIService = vertexAIService;
        _embeddingService = embeddingService;
        _chunkRepository = chunkRepository;
        _documentRepository = documentRepository;
        _conversationService = conversationService;
        _logger = logger;
        _configurationService = configurationService;
    }

    public async Task<ChatResponse> GenerateResponseAsync(ChatRequest request, Guid userId)
    {
        try
        {
            _logger.LogInfo($"Generating response for conversation {request.ConversationId}");
            var config = await _configurationService.GetRagConfigurationAsync();
            var confidenceThreshold = config.SimilarityThreshold;
            // Add user message to conversation
            await _conversationService.AddMessageAsync(request.ConversationId, "user", request.Message);

            string context = string.Empty;
            List<DocumentReference>? references = null;
            double? maxSimilarity = null;

            if (request.UseRag && request.TenantId.HasValue && request.RoleId.HasValue)
            {
                // Generate query embedding
                var queryEmbedding = await _vertexAIService.GenerateEmbeddingAsync(request.Message, config.EmbeddingModel);

                // Search for similar chunks
                var similarChunks = await _embeddingService.SearchSimilarAsync(
                    queryEmbedding,
                    request.TenantId.Value,
                    request.RoleId.Value,
                    topK: 3,
                    minSimilarity: config.SimilarityThreshold
                );

                if (similarChunks.Any())
                {
                    maxSimilarity = similarChunks.Max(c => c.Similarity);

                    if (maxSimilarity >= confidenceThreshold)
                    {
                        // Build context from retrieved chunks
                        var contextBuilder = new StringBuilder();
                        references = new List<DocumentReference>();

                        foreach (var (chunkId, similarity) in similarChunks)
                        {
                            var chunk = await _chunkRepository.GetByIdAsync(chunkId);
                            if (chunk != null)
                            {
                                var document = await _documentRepository.GetByIdAsync(chunk.DocumentId);
                                if (document != null)
                                {
                                    contextBuilder.AppendLine($"[Document: {document.FileName}]");
                                    contextBuilder.AppendLine(chunk.Content);
                                    contextBuilder.AppendLine();

                                    references.Add(new DocumentReference(
                                        DocumentId: document.Id,
                                        FileName: document.FileName,
                                        ChunkContent: chunk.Content.Substring(0, Math.Min(200, chunk.Content.Length)) + "...",
                                        SimilarityScore: similarity
                                    ));
                                }
                            }
                        }

                        context = contextBuilder.ToString();
                    }
                }
            }

            // Build prompt
            string finalPrompt;
            if (!string.IsNullOrEmpty(context))
            {
                finalPrompt = $@"You are a helpful assistant. Answer the user's question based on the following context.

                Context:
                {context}

                User Question: {request.Message}

                Please provide a clear and accurate answer based only on the information in the context. If the context doesn't contain relevant information, say 'I don't have enough information in the provided documents to answer this question.'";
            }
            else if (request.UseRag)
            {
                // RAG was requested but no relevant context found
                var noContextMessage = "No relevant information found in the knowledge base for this query.";
                var messageId = await _conversationService.AddMessageAsync(request.ConversationId, "assistant", noContextMessage);

                return new ChatResponse(
                    ConversationId: request.ConversationId,
                    MessageId: messageId.Id,
                    Content: noContextMessage,
                    References: null,
                    ConfidenceScore: maxSimilarity
                );
            }
            else
            {
                finalPrompt = request.Message;
            }

            // Generate response from Vertex AI
            var vertexRequest = new VertexAIGenerateRequest
            {
                Contents = new List<VertexAIContent>
                {
                    new()
                    {
                        Role = "user",
                        Parts = new List<VertexAIPart>
                        {
                            new() { Text = finalPrompt }
                        }
                    }
                },
                GenerationConfig = new VertexAIGenerationConfig
                {
                    Temperature = 0.7,
                    MaxOutputTokens = 2048,
                    TopP = 0.95,
                    TopK = 40
                }
            };

            var response = await _vertexAIService.GenerateTextAsync(vertexRequest, config.TextModel);
            var assistantMessage = response.Candidates.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text
                ?? throw new InvalidOperationException("No response from Vertex AI");

            // Add assistant response to conversation
            var savedMessage = await _conversationService.AddMessageAsync(request.ConversationId, "assistant", assistantMessage);

            _logger.LogInfo($"Successfully generated response for conversation {request.ConversationId}");

            return new ChatResponse(
                ConversationId: request.ConversationId,
                MessageId: savedMessage.Id,
                Content: assistantMessage,
                References: references,
                ConfidenceScore: maxSimilarity
            );
        }
        catch (Exception ex)
        {
            _logger.LogLocationWithException($"RagService():GenerateResponseAsync(): Error generating response for conversation {request.ConversationId}", ex);
            throw;
        }
    }

    public async IAsyncEnumerable<string> GenerateResponseStreamAsync(StreamChatRequest request, Guid userId)
    {
        _logger.LogInfo($"Generating streaming response for conversation {request.ConversationId}");
        var config = await _configurationService.GetRagConfigurationAsync();
        var confidenceThreshold = config.SimilarityThreshold;
        // Add user message to conversation
        await _conversationService.AddMessageAsync(request.ConversationId, "user", request.Message);

        string context = string.Empty;
        double? maxSimilarity = null;

        if (request.UseRag && request.TenantId.HasValue && request.RoleId.HasValue)
        {
            var queryEmbedding = await _vertexAIService.GenerateEmbeddingAsync(request.Message, config.EmbeddingModel);
            var similarChunks = await _embeddingService.SearchSimilarAsync(
                queryEmbedding,
                request.TenantId.Value,
                request.RoleId.Value,
                topK: 3,
                minSimilarity: config.SimilarityThreshold
            );

            if (similarChunks.Any())
            {
                maxSimilarity = similarChunks.Max(c => c.Similarity);

                if (maxSimilarity >= confidenceThreshold)
                {
                    var contextBuilder = new StringBuilder();
                    foreach (var (chunkId, _) in similarChunks)
                    {
                        var chunk = await _chunkRepository.GetByIdAsync(chunkId);
                        if (chunk != null)
                        {
                            var document = await _documentRepository.GetByIdAsync(chunk.DocumentId);
                            if (document != null)
                            {
                                contextBuilder.AppendLine($"[Document: {document.FileName}]");
                                contextBuilder.AppendLine(chunk.Content);
                                contextBuilder.AppendLine();
                            }
                        }
                    }
                    context = contextBuilder.ToString();
                }
            }
        }

        string finalPrompt;
        if (!string.IsNullOrEmpty(context))
        {
            finalPrompt = $@"You are a helpful assistant. Answer the user's question based on the following context.

            Context:
            {context}

            User Question: {request.Message}

            Please provide a clear and accurate answer based only on the information in the context.";
        }
        else if (request.UseRag)
        {
            yield return "No relevant information found in the knowledge base for this query.";
            yield break;
        }
        else
        {
            finalPrompt = request.Message;
        }

        var vertexRequest = new VertexAIGenerateRequest
        {
            Contents = new List<VertexAIContent>
            {
                new()
                {
                    Role = "user",
                    Parts = new List<VertexAIPart> { new() { Text = finalPrompt } }
                }
            }
        };

        var fullResponse = new StringBuilder();

        await foreach (var chunk in _vertexAIService.GenerateTextStreamAsync(vertexRequest, config.TextModel))
        {
            fullResponse.Append(chunk);
            yield return chunk;
        }

        // Save complete assistant response
        await _conversationService.AddMessageAsync(request.ConversationId, "assistant", fullResponse.ToString());
    }

    public async Task<List<SearchResult>> SearchDocumentsAsync(SearchDocumentsRequest request)
    {
        try
        {
            var config = await _configurationService.GetRagConfigurationAsync();
            _logger.LogInfo($"Searching documents with query: {request.Query}");

            if (!request.TenantId.HasValue || !request.RoleId.HasValue)
            {
                throw new ArgumentException("TenantId and RoleId are required for document search");
            }

            var queryEmbedding = await _vertexAIService.GenerateEmbeddingAsync(request.Query, config.EmbeddingModel);
            var similarChunks = await _embeddingService.SearchSimilarAsync(
                queryEmbedding,
                request.TenantId.Value,
                request.RoleId.Value,
                request.TopK,
                minSimilarity: config.SimilarityThreshold
            );

            var results = new List<SearchResult>();

            foreach (var (chunkId, similarity) in similarChunks)
            {
                if (similarity < request.MinSimilarity) continue;

                var chunk = await _chunkRepository.GetByIdAsync(chunkId);
                if (chunk != null)
                {
                    var document = await _documentRepository.GetByIdAsync(chunk.DocumentId);
                    if (document != null)
                    {
                        results.Add(new SearchResult(
                            DocumentId: document.Id,
                            FileName: document.OriginalFileName,
                            ChunkContent: chunk.Content,
                            SimilarityScore: similarity,
                            ChunkIndex: chunk.ChunkIndex
                        ));
                    }
                }
            }

            _logger.LogInfo($"Found {results.Count} search results");

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogLocationWithException("RagService():SearchDocumentsAsync(): Error searching documents", ex);
            throw;
        }
    }
}
