using System;
using System.Text.Json;

namespace RAGSERVERAPI.Models;


public class SearchResponse
{
    public List<SearchResults> Results { get; set; } = new();
    public string Query { get; set; } = string.Empty;
    public int TotalResults { get; set; }
    public double QueryTime { get; set; }
}

public class SearchResults
{
    public Guid DocumentId { get; set; }
    public Guid ChunkId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public int ChunkIndex { get; set; }
    public JsonDocument? DocumentMetadata { get; set; }
    public JsonDocument? ChunkMetadata { get; set; }
}
