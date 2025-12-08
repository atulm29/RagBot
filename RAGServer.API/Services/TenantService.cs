using RAGSERVERAPI.DTOs;
using RAGSERVERAPI.Models;

namespace RAGSERVERAPI.Services;

public interface ITenantService
{
    Task<List<Tenant>> GetAllTenantsAsync();
    Task<Tenant?> GetTenantByIdAsync(Guid tenantId);
    Task<List<Role>> GetTenantRolesAsync(Guid tenantId);
}
public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IRoleRepository _roleRepository;

    public TenantService(ITenantRepository tenantRepository, IRoleRepository roleRepository)
    {
        _tenantRepository = tenantRepository;
        _roleRepository = roleRepository;
    }

    public Task<List<Tenant>> GetAllTenantsAsync() => _tenantRepository.GetAllAsync();

    public Task<Tenant?> GetTenantByIdAsync(Guid tenantId) => _tenantRepository.GetByIdAsync(tenantId);

    public Task<List<Role>> GetTenantRolesAsync(Guid tenantId) => _roleRepository.GetByTenantIdAsync(tenantId);
}
