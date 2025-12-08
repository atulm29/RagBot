
using System.Data;
using Dapper;
using RAGSERVERAPI.Models;
using RAGSERVERAPI.Services;
public interface IDocumentRepository
{
    Task<Document> CreateAsync(Document document);
    Task<Document?> GetByIdAsync(Guid documentId);
    Task<List<Document>> GetByUserIdAsync(Guid userId, Guid? tenantId = null);
    Task UpdateStatusAsync(Guid documentId, string status);
    Task<bool> DeleteAsync(Guid documentId);
}

public class DocumentRepository : IDocumentRepository
{
    private readonly DataContext _context;

    public DocumentRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Document> CreateAsync(Document document)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            INSERT INTO documents (id, tenantid, roleid, userid, filename, originalfilename, bucketpath, 
                                  contenttype, filesize, gcsuri, ispublic, status, metadata, createdat, updatedat)
            VALUES (@Id, @TenantId, @RoleId, @UserId, @FileName, @OriginalFileName, @BucketPath,
                    @ContentType, @FileSize, @GcsUri, @IsPublic, @Status, CAST(@Metadata AS jsonb), @CreatedAt, @UpdatedAt)
            RETURNING *";

        return await connection.QuerySingleAsync<Document>(sql, document);
    }

    public async Task<Document?> GetByIdAsync(Guid documentId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM documents WHERE id = @DocumentId";
        return await connection.QueryFirstOrDefaultAsync<Document>(sql, new { DocumentId = documentId });
    }

    public async Task<List<Document>> GetByUserIdAsync(Guid userId, Guid? tenantId = null)
    {
        using var connection = _context.CreateConnection();
        var sql = tenantId.HasValue
            ? "SELECT * FROM documents WHERE userid = @UserId AND tenantid = @TenantId ORDER BY createdat DESC"
            : "SELECT * FROM documents WHERE userid = @UserId ORDER BY createdat DESC";

        var result = await connection.QueryAsync<Document>(sql, new { UserId = userId, TenantId = tenantId });
        return result.ToList();
    }

    public async Task UpdateStatusAsync(Guid documentId, string status)
    {
        using var connection = _context.CreateConnection();
        var sql = "UPDATE documents SET status = @Status, updatedat = @UpdatedAt WHERE id = @DocumentId";
        await connection.ExecuteAsync(sql, new { DocumentId = documentId, Status = status, UpdatedAt = DateTime.UtcNow });
    }

    public async Task<bool> DeleteAsync(Guid documentId)
    {
        using var connection = _context.CreateConnection();
        var sql = "DELETE FROM documents WHERE id = @DocumentId";
        var affected = await connection.ExecuteAsync(sql, new { DocumentId = documentId });
        return affected > 0;
    }

}
