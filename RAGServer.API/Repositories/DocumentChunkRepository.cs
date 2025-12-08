using System.Data;
using Dapper;
using RAGSERVERAPI.Models;
using RAGSERVERAPI.Services;

public interface IDocumentChunkRepository
{
    Task<DocumentChunk> CreateAsync(DocumentChunk chunk);
    Task<List<DocumentChunk>> GetByDocumentIdAsync(Guid documentId);
    Task<DocumentChunk?> GetByIdAsync(Guid chunkId);
    Task<bool> DeleteDocumentChuncks(Guid documentId);
}

public class DocumentChunkRepository : IDocumentChunkRepository
{
    private readonly DataContext _context;

    public DocumentChunkRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<DocumentChunk> CreateAsync(DocumentChunk chunk)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            INSERT INTO documentchunks (id, documentid, chunkindex, content, tokencount, metadata, createdat)
            VALUES (@Id, @DocumentId, @ChunkIndex, @Content, @TokenCount, CAST(@Metadata AS jsonb), @CreatedAt)
            RETURNING *";

        return await connection.QuerySingleAsync<DocumentChunk>(sql, chunk);
    }

    public async Task<List<DocumentChunk>> GetByDocumentIdAsync(Guid documentId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM documentchunks WHERE documentid = @DocumentId ORDER BY chunkindex";
        var result = await connection.QueryAsync<DocumentChunk>(sql, new { DocumentId = documentId });
        return result.ToList();
    }

    public async Task<DocumentChunk?> GetByIdAsync(Guid chunkId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM documentchunks WHERE id = @ChunkId";
        return await connection.QueryFirstOrDefaultAsync<DocumentChunk>(sql, new { ChunkId = chunkId });
    }

    public async Task<bool> DeleteDocumentChuncks(Guid documentId)
    {
        try
        {
            var con = _context.CreateConnection();
            var sql = "DELETE FROM chunks WHERE documentid = @Id";
            var rowsAffected = await con.ExecuteAsync(sql, new { Id = documentId });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            //_logger.LogLocationWithException("EmbeddingRepository => DeleteDocumentChuncks =>", ex);
            Console.WriteLine($"Error deleting DeleteDocumentChuncks: {ex.Message}");
            return false;
        }
    }
}
