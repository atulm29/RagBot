using System.Data;
using Dapper;
using RAGSERVERAPI.Models;
using RAGSERVERAPI.Services;

public interface IConfigurationRepository
{
    Task<Configuration?> GetByKeyAsync(string key);
    Task<Configuration> CreateAsync(Configuration configuration);
    Task UpdateAsync(Configuration configuration);
    Task<bool> DeleteAsync(string key);
}

public class ConfigurationRepository : IConfigurationRepository
{
    private readonly DataContext _context;

    public ConfigurationRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Configuration?> GetByKeyAsync(string key)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM configurations WHERE key = @Key";
        return await connection.QueryFirstOrDefaultAsync<Configuration>(sql, new { Key = key });
    }

    public async Task<Configuration> CreateAsync(Configuration configuration)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            INSERT INTO configurations (id, key, value, description, createdat, updatedat)
            VALUES (@Id, @Key, @Value, @Description, @CreatedAt, @UpdatedAt)
            RETURNING *";

        return await connection.QuerySingleAsync<Configuration>(sql, configuration);
    }

    public async Task UpdateAsync(Configuration configuration)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            UPDATE configurations 
            SET value = @Value, description = @Description, updatedat = @UpdatedAt
            WHERE key = @Key";

        await connection.ExecuteAsync(sql, configuration);
    }

    public async Task<bool> DeleteAsync(string key)
    {
        using var connection = _context.CreateConnection();
        var sql = "DELETE FROM configurations WHERE key = @Key";
        var affected = await connection.ExecuteAsync(sql, new { Key = key });
        return affected > 0;
    }
}
