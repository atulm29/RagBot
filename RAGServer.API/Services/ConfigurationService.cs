using RAGSERVERAPI.DTOs;
using RAGSERVERAPI.Models;
using System.Text.Json;

namespace RAGSERVERAPI.Services;

public interface IConfigurationService
{
    Task<RagConfigurationDto> GetRagConfigurationAsync();
    Task UpdateRagConfigurationAsync(RagConfigurationDto config);
    Task ResetRagConfigurationAsync();
}

public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationRepository _configurationRepository;
    private readonly ILogger<ConfigurationService> _logger;
    private static readonly RagConfigurationDto DefaultConfig = new()
    {
        ChunkSize = 1000,
        ChunkOverlap = 200,
        ChunkingStrategy = "paragraph-based",
        EmbeddingModel = "text-embedding-004",
        TopK = 5,
        SimilarityThreshold = 0.7,
        RetrievalMethod = "similarity-search",
        MaxCharsPerInstance = 12000,
        TextModel = "gemini-2.0-flash",
        EmbeddingBatchSize = 5,
        MaxRetryAttempts = 5
    };

    public ConfigurationService(
        IConfigurationRepository configurationRepository,
        ILogger<ConfigurationService> logger)
    {
        _configurationRepository = configurationRepository;
        _logger = logger;
    }

    public async Task<RagConfigurationDto> GetRagConfigurationAsync()
    {
        try
        {
            var config = await _configurationRepository.GetByKeyAsync("rag_configuration");

            if (config == null)
            {
                // Return default configuration if none exists
                return DefaultConfig;
            }

            return JsonSerializer.Deserialize<RagConfigurationDto>(config.Value)
                ?? DefaultConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting RAG configuration");
            return DefaultConfig;
        }
    }

    public async Task UpdateRagConfigurationAsync(RagConfigurationDto config)
    {
        try
        {
            //ValidateConfiguration(config);

            var configJson = JsonSerializer.Serialize(config);

            var existingConfig = await _configurationRepository.GetByKeyAsync("rag_configuration");

            if (existingConfig == null)
            {
                await _configurationRepository.CreateAsync(new Configuration
                {
                    Id = Guid.NewGuid(),
                    Key = "rag_configuration",
                    Value = configJson,
                    Description = "RAG system configuration settings",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existingConfig.Value = configJson;
                existingConfig.UpdatedAt = DateTime.UtcNow;
                await _configurationRepository.UpdateAsync(existingConfig);
            }

            _logger.LogInformation("RAG configuration updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating RAG configuration");
            throw;
        }
    }

    public async Task ResetRagConfigurationAsync()
    {
        try
        {
            await UpdateRagConfigurationAsync(DefaultConfig);
            _logger.LogInformation("RAG configuration reset to defaults");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting RAG configuration");
            throw;
        }
    }

    private void ValidateConfiguration(RagConfigurationDto config)
    {
        if (config.ChunkSize < 100 || config.ChunkSize > 2000)
            throw new ArgumentException("ChunkSize must be between 100 and 2000");

        if (config.ChunkOverlap < 0 || config.ChunkOverlap > 500)
            throw new ArgumentException("ChunkOverlap must be between 0 and 500");

        if (config.TopK < 1 || config.TopK > 20)
            throw new ArgumentException("TopK must be between 1 and 20");

        if (config.SimilarityThreshold < 0 || config.SimilarityThreshold > 1)
            throw new ArgumentException("SimilarityThreshold must be between 0 and 1");

        var validStrategies = new[] { "paragraph-based", "sentence-based", "fixed-size", "semantic" };
        if (!validStrategies.Contains(config.ChunkingStrategy))
            throw new ArgumentException($"ChunkingStrategy must be one of: {string.Join(", ", validStrategies)}");

        var validMethods = new[] { "similarity-search", "mmr", "hybrid", "rerank" };
        if (!validMethods.Contains(config.RetrievalMethod))
            throw new ArgumentException($"RetrievalMethod must be one of: {string.Join(", ", validMethods)}");
    }
}
