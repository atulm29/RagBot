using Pgvector;

namespace RAGSERVERAPI.Models;

public class Embedding
{
    public Guid Id { get; set; }
    public Guid ChunkId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid TenantId { get; set; }
    public Guid RoleId { get; set; }
    public float[] EmbeddingVector { get; set; } = Array.Empty<float>();
    public string ModelName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    //public Vector Embedding => new Vector(EmbeddingVector);
}
