using System.Data;
using Dapper;
using RAGSERVERAPI.Models;
using RAGSERVERAPI.Services;

public interface IRoleRepository
{
    Task<List<Role>> GetByTenantIdAsync(Guid tenantId);
    Task<Role?> GetByIdAsync(Guid roleId);
}

public class RoleRepository : IRoleRepository
{
    private readonly DataContext _context;

    public RoleRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<List<Role>> GetByTenantIdAsync(Guid tenantId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM roles WHERE tenantid = @TenantId AND isactive = true ORDER BY name";
        var result = await connection.QueryAsync<Role>(sql, new { TenantId = tenantId });
        return result.ToList();
    }

    public async Task<Role?> GetByIdAsync(Guid roleId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM roles WHERE id = @RoleId";
        return await connection.QueryFirstOrDefaultAsync<Role>(sql, new { RoleId = roleId });
    }
}
