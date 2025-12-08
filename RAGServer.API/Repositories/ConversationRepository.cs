
using System.Data;
using Dapper;
using RAGSERVERAPI.Models;
using RAGSERVERAPI.Services;
public interface IConversationRepository
{
    Task<Conversation> CreateAsync(Conversation conversation);
    Task<Conversation?> GetByIdAsync(Guid conversationId);
    Task<List<Conversation>> GetByUserIdAsync(Guid userId);
    Task<List<ConversationMessage>> GetMessagesAsync(Guid conversationId);
    Task<ConversationMessage> AddMessageAsync(ConversationMessage message);
    Task<bool> DeleteAsync(Guid conversationId);
}
public class ConversationRepository : IConversationRepository
{
    private readonly DataContext _context;

    public ConversationRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Conversation> CreateAsync(Conversation conversation)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            INSERT INTO conversations (id, userid, tenantid, roleid, title, isactive, createdat, updatedat)
            VALUES (@Id, @UserId, @TenantId, @RoleId, @Title, @IsActive, @CreatedAt, @UpdatedAt)
            RETURNING *";

        return await connection.QuerySingleAsync<Conversation>(sql, conversation);
    }

    public async Task<Conversation?> GetByIdAsync(Guid conversationId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM conversations WHERE id = @ConversationId";
        return await connection.QueryFirstOrDefaultAsync<Conversation>(sql, new { ConversationId = conversationId });
    }

    public async Task<List<Conversation>> GetByUserIdAsync(Guid userId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM conversations WHERE userid = @UserId ORDER BY updatedat DESC";
        var result = await connection.QueryAsync<Conversation>(sql, new { UserId = userId });
        return result.ToList();
    }

    public async Task<List<ConversationMessage>> GetMessagesAsync(Guid conversationId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM conversationmessages WHERE conversationid = @ConversationId ORDER BY createdat ASC";
        var result = await connection.QueryAsync<ConversationMessage>(sql, new { ConversationId = conversationId });
        return result.ToList();
    }

    public async Task<ConversationMessage> AddMessageAsync(ConversationMessage message)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            INSERT INTO conversationmessages (id, conversationid, role, content, metadata, createdat)
            VALUES (@Id, @ConversationId, @Role, @Content, CAST(@Metadata AS jsonb), @CreatedAt)
            RETURNING *";

        return await connection.QuerySingleAsync<ConversationMessage>(sql, message);
    }

    public async Task<bool> DeleteAsync(Guid conversationId)
    {
        using var connection = _context.CreateConnection();
        var sql = "DELETE FROM conversations WHERE id = @ConversationId";
        var affected = await connection.ExecuteAsync(sql, new { ConversationId = conversationId });
        return affected > 0;
    }
}
