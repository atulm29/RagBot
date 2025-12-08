using System.Data;
using Dapper;
using RAGSERVERAPI.Models;
using RAGSERVERAPI.Services;
public interface ITenantRepository
{
    Task<List<Tenant>> GetAllAsync();
    Task<Tenant?> GetByIdAsync(Guid tenantId);
    Task<Tenant?> GetByCodeAsync(string code);
}

public class TenantRepository : ITenantRepository
{
    private readonly DataContext _context;

    public TenantRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<List<Tenant>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM tenants WHERE isactive = true ORDER BY name";
        var result = await connection.QueryAsync<Tenant>(sql);
        return result.ToList();
    }

    public async Task<Tenant?> GetByIdAsync(Guid tenantId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM tenants WHERE id = @TenantId";
        return await connection.QueryFirstOrDefaultAsync<Tenant>(sql, new { TenantId = tenantId });
    }

    public async Task<Tenant?> GetByCodeAsync(string code)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM tenants WHERE code = @Code";
        return await connection.QueryFirstOrDefaultAsync<Tenant>(sql, new { Code = code });
    }
}
