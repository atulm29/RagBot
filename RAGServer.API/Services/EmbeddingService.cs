
using RAGSERVERAPI.DTOs;
using RAGSERVERAPI.Models;

namespace RAGSERVERAPI.Services;

public interface IEmbeddingService
{
    Task<Guid> CreateEmbeddingAsync(Guid chunkId, Guid documentId, Guid tenantId, Guid roleId, string content);
    Task<List<(Guid ChunkId, double Similarity)>> SearchSimilarAsync(float[] queryEmbedding, Guid tenantId, Guid roleId, int topK = 5, double minSimilarity = 0.6);
}
public class EmbeddingService : IEmbeddingService
{
    private readonly IVertexAIService _vertexAIService;
    private readonly IEmbeddingRepository _embeddingRepository;
    private readonly ILogger _logger;
    private readonly IConfigurationService _configurationService;
    public EmbeddingService(IVertexAIService vertexAIService, IEmbeddingRepository embeddingRepository,
        ILogger logger, IConfigurationService configurationService)
    {
        _vertexAIService = vertexAIService;
        _embeddingRepository = embeddingRepository;
        _logger = logger;
        _configurationService = configurationService;
    }

    public async Task<Guid> CreateEmbeddingAsync(Guid chunkId, Guid documentId, Guid tenantId, Guid roleId, string content)
    {
        try
        {
            var config = await _configurationService.GetRagConfigurationAsync();
            _logger.LogInfo($"Generating embedding for chunk {chunkId}");
            await Task.Delay(500);
            var embeddingVector = await _vertexAIService.GenerateEmbeddingAsync(content, config.EmbeddingModel);

            var embedding = new Embedding
            {
                Id = Guid.NewGuid(),
                ChunkId = chunkId,
                DocumentId = documentId,
                TenantId = tenantId,
                RoleId = roleId,
                EmbeddingVector = embeddingVector,
                ModelName = config.EmbeddingModel,
                CreatedAt = DateTime.UtcNow
            };

            await _embeddingRepository.CreateAsync(embedding);

            _logger.LogInfo($"Successfully created embedding {embedding.Id} for chunk {chunkId}");

            return embedding.Id;
        }
        catch (Exception ex)
        {
            _logger.LogLocationWithException($"EmbeddingService: CreateEmbeddingAsync: Error creating embedding for chunk {chunkId}", ex);
            throw;
        }
    }

    public async Task<List<(Guid ChunkId, double Similarity)>> SearchSimilarAsync(
        float[] queryEmbedding,
        Guid tenantId,
        Guid roleId,
        int topK = 5,
        double minSimilarity = 0.6)
    {
        try
        {
            _logger.LogInfo($"Searching similar embeddings for tenant {tenantId}, role {roleId}");

            var results = await _embeddingRepository.SearchSimilarAsync(
                queryEmbedding,
                tenantId,
                roleId,
                topK,
                minSimilarity
            );

            _logger.LogInfo($"Found {results.Count} similar embeddings");

            return results.Select(r => (r.ChunkId, r.Similarity)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogLocationWithException("EmbeddingService: SearchSimilarAsync: ", ex);
            throw;
        }
    }
}
