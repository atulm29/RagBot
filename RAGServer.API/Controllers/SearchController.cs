using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RAGSERVERAPI.DTOs;
using RAGSERVERAPI.Services;
using RAGSERVERAPI.Models;

namespace RAGSERVERAPI.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IRagService _ragService;
    private readonly IValidator<SearchDocumentsRequest> _searchValidator;
    public SearchController(
        IRagService ragService,
        IValidator<SearchDocumentsRequest> searchValidator)
    {
        _ragService = ragService;
        _searchValidator = searchValidator;
    }

    [HttpPost("documents")]
    public async Task<ActionResult<List<SearchResults>>> SearchDocuments([FromBody] SearchDocumentsRequest request)
    {
        var validationResult = await _searchValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var results = await _ragService.SearchDocumentsAsync(request);
        return Ok(results);
    }
}
