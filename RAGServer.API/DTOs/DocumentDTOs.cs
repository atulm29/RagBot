namespace RAGSERVERAPI.DTOs;

public record UploadDocumentRequest(
    string FileName,
    string ContentType,
    byte[] FileContent,
    Guid TenantId,
    Guid RoleId,
    bool IsPublic = false
);

public record DocumentUploadResponse(
    Guid DocumentId,
    string FileName,
    string GcsUri,
    string Status
);

public record DocumentListResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSize,
    string Status,
    DateTime CreatedAt
);

public record SearchDocumentsRequest(
    string Query,
    Guid? TenantId = null,
    Guid? RoleId = null,
    int TopK = 5,
    double MinSimilarity = 0.6
);

public record SearchResult(
    Guid DocumentId,
    string FileName,
    string ChunkContent,
    double SimilarityScore,
    int ChunkIndex
);
