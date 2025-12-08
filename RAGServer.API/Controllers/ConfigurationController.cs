using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RAGSERVERAPI.DTOs;
using RAGSERVERAPI.Services;

namespace RAGSERVERAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfigurationController : ControllerBase
{
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(
        IConfigurationService configurationService,
        ILogger<ConfigurationController> logger)
    {
        _configurationService = configurationService;
        _logger = logger;
    }

    [HttpGet("rag")]
    public async Task<IActionResult> GetRagConfiguration()
    {
        try
        {
            var config = await _configurationService.GetRagConfigurationAsync();
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving RAG configuration");
            return StatusCode(500, new { message = "Error retrieving configuration" });
        }
    }

    [HttpPut("rag")]
    public async Task<IActionResult> UpdateRagConfiguration([FromBody] RagConfigurationDto config)
    {
        try
        {
            await _configurationService.UpdateRagConfigurationAsync(config);
            return Ok(new { message = "Configuration updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating RAG configuration");
            return StatusCode(500, new { message = "Error updating configuration" });
        }
    }

    [HttpPost("rag/reset")]
    public async Task<IActionResult> ResetRagConfiguration()
    {
        try
        {
            await _configurationService.ResetRagConfigurationAsync();
            return Ok(new { message = "Configuration reset to defaults" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting RAG configuration");
            return StatusCode(500, new { message = "Error resetting configuration" });
        }
    }
}
