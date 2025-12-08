using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RAGSERVERAPI.Models;
using RAGSERVERAPI.Repositories;
using RAGSERVERAPI.Services;

namespace RAGSERVERAPI.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TenantController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpGet]
    public async Task<ActionResult> GetTenants()
    {
        var tenants = await _tenantService.GetAllTenantsAsync();
        return Ok(tenants);
    }

    [HttpGet("{tenantId}")]
    public async Task<ActionResult> GetTenant(Guid tenantId)
    {
        var tenant = await _tenantService.GetTenantByIdAsync(tenantId);
        if (tenant == null)
        {
            return NotFound();
        }
        return Ok(tenant);
    }

    [HttpGet("{tenantId}/roles")]
    public async Task<ActionResult> GetTenantRoles(Guid tenantId)
    {
        var roles = await _tenantService.GetTenantRolesAsync(tenantId);
        return Ok(roles);
    }
}
