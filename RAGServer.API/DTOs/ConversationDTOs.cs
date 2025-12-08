namespace RAGSERVERAPI.DTOs;
public record CreateConversationRequest(
    Guid TenantId,
    Guid RoleId,
    string? Title = null
);

public record ConversationResponse(
    Guid Id,
    string? Title,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<MessageResponse> Messages
);

public record MessageResponse(
    Guid Id,
    string Role,
    string Content,
    DateTime CreatedAt
);
