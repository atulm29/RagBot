using System.Text.Json.Serialization;

namespace RAGSERVERAPI.DTOs;

public record VertexAIGenerateRequest
{
    [JsonPropertyName("contents")]
    public List<VertexAIContent> Contents { get; init; } = new();

    [JsonPropertyName("generationConfig")]
    public VertexAIGenerationConfig? GenerationConfig { get; init; }
}

public record VertexAIContent
{
    [JsonPropertyName("role")]
    public string Role { get; init; } = string.Empty;

    [JsonPropertyName("parts")]
    public List<VertexAIPart> Parts { get; init; } = new();
}

public record VertexAIPart
{
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
}

public record VertexAIGenerationConfig
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; init; } = 0.7;

    [JsonPropertyName("maxOutputTokens")]
    public int MaxOutputTokens { get; init; } = 2048;

    [JsonPropertyName("topP")]
    public double TopP { get; init; } = 0.95;

    [JsonPropertyName("topK")]
    public int TopK { get; init; } = 40;
}


public record VertexAIGenerateResponse
{
    [JsonPropertyName("candidates")]
    public List<VertexAICandidate> Candidates { get; init; } = new();

    [JsonPropertyName("usageMetadata")]
    public VertexAIUsageMetadata? UsageMetadata { get; init; }

    [JsonPropertyName("modelVersion")]
    public string? ModelVersion { get; init; }

    [JsonPropertyName("createTime")]
    public string? CreateTime { get; init; }

    [JsonPropertyName("responseId")]
    public string? ResponseId { get; init; }
}

public record VertexAICandidate
{
    [JsonPropertyName("content")]
    public VertexAIContent Content { get; init; } = new();

    [JsonPropertyName("finishReason")]
    public string FinishReason { get; init; } = string.Empty;
}

public record VertexAIUsageMetadata
{
    [JsonPropertyName("promptTokenCount")]
    public int PromptTokenCount { get; init; }

    [JsonPropertyName("candidatesTokenCount")]
    public int CandidatesTokenCount { get; init; }

    [JsonPropertyName("totalTokenCount")]
    public int TotalTokenCount { get; init; }

    [JsonPropertyName("trafficType")]
    public string? TrafficType { get; init; }

    [JsonPropertyName("promptTokensDetails")]
    public List<TokenDetail>? PromptTokensDetails { get; init; }

    [JsonPropertyName("candidatesTokensDetails")]
    public List<TokenDetail>? CandidatesTokensDetails { get; init; }
}

public record TokenDetail
{
    [JsonPropertyName("modality")]
    public string? Modality { get; init; }

    [JsonPropertyName("tokenCount")]
    public int TokenCount { get; init; }
}

public record VertexAIEmbeddingRequest
{
    public List<VertexAIEmbeddingInstance> Instances { get; init; } = new();
}

public record VertexAIEmbeddingInstance
{
    public string Content { get; init; } = string.Empty;
}

public class EmbeddingResponse
{
    public List<Prediction> Predictions { get; set; }
}

public class Prediction
{
    public Embeddings Embeddings { get; set; }
}

public class Embeddings
{
    public List<float> Values { get; set; }
}
