using System.Data;
using Dapper;
using RAGSERVERAPI.Models;
using RAGSERVERAPI.Services;
using Pgvector;
using Npgsql;
public interface IEmbeddingRepository
{
    Task<Embedding> CreateAsync(Embedding embedding);
    Task<List<SimilarityResult>> SearchSimilarAsync(float[] queryEmbedding, Guid tenantId, Guid roleId, int topK = 5, double minSimilarity = 0.6);

    Task<bool> DeleteDocumentEmbeddings(Guid documentId);

}
public class EmbeddingRepository : IEmbeddingRepository
{
    private readonly DataContext _context;
    private readonly RAGSERVERAPI.Services.ILogger _logger;
    public EmbeddingRepository(DataContext context, RAGSERVERAPI.Services.ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Embedding> CreateAsync(Embedding embedding)
    {
        using var _conn = _context.CreateConnection();
        const string sql = @"
            INSERT INTO embeddings
            (id, chunkid, documentid, tenantid, roleid, embedding, modelname, createdat)
            VALUES
            (@Id, @ChunkId, @DocumentId, @TenantId, @RoleId, @Embedding, @ModelName, @CreatedAt)
            RETURNING *;
        ";

        var parameters = new
        {
            embedding.Id,
            embedding.ChunkId,
            embedding.DocumentId,
            embedding.TenantId,
            embedding.RoleId,
            Embedding = embedding.EmbeddingVector,
            embedding.ModelName,
            embedding.CreatedAt
        };

        return await _conn.QuerySingleAsync<Embedding>(sql, parameters);
    }
    public async Task<List<SimilarityResult>> SearchSimilarAsync(float[] queryEmbedding, Guid tenantId, Guid roleId, int topK = 5, double minSimilarity = 0.6)
    {
        var connection = _context.CreateConnection();
        try
        {
            await connection.OpenAsync();

            // Check dimensions of stored embeddings using vector_dims()
            var dimensionCheck = await connection.ExecuteScalarAsync<int?>(
                @"SELECT vector_dims(embedding) 
                FROM embeddings 
                WHERE tenantid = @TenantId AND roleid = @RoleId 
                LIMIT 1",
                new { TenantId = tenantId, RoleId = roleId }
            );

            Console.WriteLine($"Query embedding dimensions: {queryEmbedding.Length}");
            Console.WriteLine($"Stored embedding dimensions: {dimensionCheck}");

            if (dimensionCheck.HasValue && dimensionCheck.Value != queryEmbedding.Length)
            {
                throw new Exception($"Dimension mismatch: Query has {queryEmbedding.Length} dimensions, stored embeddings have {dimensionCheck.Value}");
            }

            if (!dimensionCheck.HasValue)
            {
                Console.WriteLine($"No embeddings found for tenant {tenantId}, role {roleId}");
                return new List<SimilarityResult>();
            }

            var pgVector = new Vector(queryEmbedding);

            var sql = @"
                SELECT 
                    chunkid AS ChunkId,
                    documentid AS DocumentId,
                    (1 - (embedding <=> @QueryEmbedding)) AS Similarity
                FROM embeddings
                WHERE tenantid = @TenantId 
                AND roleid = @RoleId
                ORDER BY embedding <=> @QueryEmbedding
                LIMIT @TopK";

            var result = await connection.QueryAsync<SimilarityResult>(
                sql,
                new
                {
                    QueryEmbedding = pgVector,
                    TenantId = tenantId,
                    RoleId = roleId,
                    TopK = topK
                });

            var allResults = result.ToList();

            Console.WriteLine($"Found {allResults.Count} results before filtering");
            foreach (var r in allResults.Take(5))
            {
                Console.WriteLine($"  ChunkId: {r.ChunkId}, Similarity: {r.Similarity:F4}");
            }

            var filtered = allResults.Where(r => r.Similarity >= minSimilarity).ToList();
            Console.WriteLine($"After filtering (>= {minSimilarity}): {filtered.Count} results");
            var cleanResults = filtered.Select(x => new SimilarityResult
            {
                ChunkId = x.ChunkId,
                DocumentId = x.DocumentId,
                Similarity = x.Similarity
            }).ToList();
            return cleanResults;
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }
    public async Task<bool> DeleteDocumentEmbeddings(Guid documentId)
    {
        try
        {
            var con = _context.CreateConnection();
            var sql = "DELETE FROM embeddings WHERE documentid = @Id";
            var rowsAffected = await con.ExecuteAsync(sql, new { Id = documentId });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            //_logger.LogLocationWithException("EmbeddingRepository => DeleteDocumentEmbeddings =>", ex);
            Console.WriteLine($"Error deleting DeleteDocumentEmbeddings: {ex.Message}");
            return false;
        }
    }
}
