
namespace RAGSERVERAPI.Models;

public class Document
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RoleId { get; set; }
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string BucketPath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string GcsUri { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public string Status { get; set; } = DocumentStatus.Processing.ToString();
    public string? Metadata { get; set; } // JSON
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum DocumentStatus
{
    Uploading,
    Processing,
    Indexed,
    Error,
}
