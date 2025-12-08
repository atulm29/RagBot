namespace RAGSERVERAPI.DTOs;
public record ChatRequest(
    Guid ConversationId,
    string Message,
    Guid? TenantId = null,
    Guid? RoleId = null,
    bool UseRag = true
);

public record ChatResponse(
    Guid ConversationId,
    Guid MessageId,
    string Content,
    List<DocumentReference>? References = null,
    double? ConfidenceScore = null
);

public record DocumentReference(
    Guid DocumentId,
    string FileName,
    string ChunkContent,
    double SimilarityScore
);

public record StreamChatRequest(
    Guid ConversationId,
    string Message,
    Guid? TenantId = null,
    Guid? RoleId = null,
    bool UseRag = true
);
