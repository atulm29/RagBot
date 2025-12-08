
namespace RAGSERVERAPI.Models;
public class ConversationMessage
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public string Role { get; set; } = string.Empty; // user, assistant, system
    public string Content { get; set; } = string.Empty;
    public string? Metadata { get; set; } // JSON
    public DateTime CreatedAt { get; set; }
}
