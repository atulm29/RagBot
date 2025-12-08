using System.Text.Json.Serialization;

namespace RAGSERVERAPI.DTOs;

public class RagConfigurationDto
{
    [JsonPropertyName("chunkSize")]
    public int ChunkSize { get; set; }

    [JsonPropertyName("chunkOverlap")]
    public int ChunkOverlap { get; set; }

    [JsonPropertyName("chunkingStrategy")]
    public string ChunkingStrategy { get; set; } = string.Empty;

    [JsonPropertyName("embeddingModel")]
    public string EmbeddingModel { get; set; } = string.Empty;

    [JsonPropertyName("topK")]
    public int TopK { get; set; }

    [JsonPropertyName("similarityThreshold")]
    public double SimilarityThreshold { get; set; }

    [JsonPropertyName("retrievalMethod")]
    public string RetrievalMethod { get; set; } = string.Empty;

    [JsonPropertyName("maxCharsPerInstance")]
    public int MaxCharsPerInstance { get; set; }

    [JsonPropertyName("textModel")]
    public string TextModel { get; set; } = string.Empty;

    [JsonPropertyName("embeddingBatchSize")]
    public int EmbeddingBatchSize { get; set; }

    [JsonPropertyName("maxRetryAttempts")]
    public int MaxRetryAttempts { get; set; }
}
