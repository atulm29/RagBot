namespace RAGSERVERAPI.Models;

public class SimilarityResult
{
    public Guid ChunkId { get; set; }
    public Guid DocumentId { get; set; }
    public double Similarity { get; set; }
}

public class SearchRequest
{
    public string Query { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public Guid? RoleId { get; set; }
    public int TopK { get; set; } = 5;
    public double SimilarityThreshold { get; set; } = 0.7;
}
public class SearchResultDto
{
    public Guid DocumentId { get; set; }
    public Guid ChunkId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public int ChunkIndex { get; set; }
    public string? DocumentMetadata { get; set; }
    public string? ChunkMetadata { get; set; }
}
