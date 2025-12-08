
using RAGSERVERAPI.DTOs;
using RAGSERVERAPI.Models;
namespace RAGSERVERAPI.Services;


public interface IConversationService
{
    Task<Conversation> CreateConversationAsync(CreateConversationRequest request, Guid userId);
    Task<ConversationResponse> GetConversationAsync(Guid conversationId, Guid userId);
    Task<List<ConversationResponse>> GetUserConversationsAsync(Guid userId);
    Task<ConversationMessage> AddMessageAsync(Guid conversationId, string role, string content);
    Task<bool> DeleteConversationAsync(Guid conversationId, Guid userId);
}
public class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ILogger _logger;

    public ConversationService(IConversationRepository conversationRepository, ILogger logger)
    {
        _conversationRepository = conversationRepository;
        _logger = logger;
    }

    public async Task<Conversation> CreateConversationAsync(CreateConversationRequest request, Guid userId)
    {
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = request.TenantId,
            RoleId = request.RoleId,
            Title = request.Title ?? "New Conversation",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _conversationRepository.CreateAsync(conversation);
    }

    public async Task<ConversationResponse> GetConversationAsync(Guid conversationId, Guid userId)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation == null || conversation.UserId != userId)
        {
            throw new UnauthorizedAccessException("Conversation not found or access denied");
        }

        var messages = await _conversationRepository.GetMessagesAsync(conversationId);

        return new ConversationResponse(
            Id: conversation.Id,
            Title: conversation.Title,
            CreatedAt: conversation.CreatedAt,
            UpdatedAt: conversation.UpdatedAt,
            Messages: messages.Select(m => new MessageResponse(
                Id: m.Id,
                Role: m.Role,
                Content: m.Content,
                CreatedAt: m.CreatedAt
            )).ToList()
        );
    }

    public async Task<List<ConversationResponse>> GetUserConversationsAsync(Guid userId)
    {
        var conversations = await _conversationRepository.GetByUserIdAsync(userId);

        var result = new List<ConversationResponse>();
        foreach (var conv in conversations)
        {
            var messages = await _conversationRepository.GetMessagesAsync(conv.Id);
            result.Add(new ConversationResponse(
                Id: conv.Id,
                Title: conv.Title,
                CreatedAt: conv.CreatedAt,
                UpdatedAt: conv.UpdatedAt,
                Messages: messages.Select(m => new MessageResponse(
                    Id: m.Id,
                    Role: m.Role,
                    Content: m.Content,
                    CreatedAt: m.CreatedAt
                )).ToList()
            ));
        }

        return result;
    }

    public async Task<ConversationMessage> AddMessageAsync(Guid conversationId, string role, string content)
    {
        var message = new ConversationMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Role = role,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        return await _conversationRepository.AddMessageAsync(message);
    }

    public async Task<bool> DeleteConversationAsync(Guid conversationId, Guid userId)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation == null || conversation.UserId != userId)
        {
            return false;
        }

        return await _conversationRepository.DeleteAsync(conversationId);
    }
}
