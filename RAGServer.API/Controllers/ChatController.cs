using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RAGSERVERAPI.DTOs;
using RAGSERVERAPI.Services;
using Dapper;
using RAGSERVERAPI.Models;

namespace RAGSERVERAPI.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IRagService _ragService;
    private readonly IValidator<ChatRequest> _chatValidator;
    private readonly DataContext _context;
    private readonly ISearchService _searchService;

    public ChatController(IRagService ragService, IValidator<ChatRequest> chatValidator, DataContext context, ISearchService searchService)
    {
        _ragService = ragService;
        _chatValidator = chatValidator;
        _context = context;
        _searchService = searchService;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
    {
        var validationResult = await _chatValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _ragService.GenerateResponseAsync(request, userId);
        return Ok(response);
    }

    [HttpPost("stream")]
    public async Task StreamChat([FromBody] StreamChatRequest request)
    {
        try
        {
            Response.ContentType = "text/event-stream";
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await foreach (var chunk in _ragService.GenerateResponseStreamAsync(request, userId))
            {
                await Response.WriteAsync($"data: {chunk}\n\n");
                await Response.Body.FlushAsync();
            }

            await Response.WriteAsync("data: [DONE]\n\n");
            await Response.Body.FlushAsync();
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error in streaming chat");
            await Response.WriteAsync($"data: {{\"error\": \"An error occurred\"}}\n\n");
            await Response.Body.FlushAsync();
        }
    }

    [HttpPost("semantic")]
    [ProducesResponseType(typeof(SearchResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SemanticSearch([FromBody] SearchRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new { error = "Query text is required" });
            }

            if (request.TopK <= 0 || request.TopK > 100)
            {
                return BadRequest(new { error = "TopK must be between 1 and 100" });
            }

            if (request.SimilarityThreshold < 0 || request.SimilarityThreshold > 1)
            {
                return BadRequest(new { error = "SimilarityThreshold must be between 0 and 1" });
            }

            var response = await _searchService.SearchAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            // _logger.LogError(ex, "Error in semantic search endpoint");
            return StatusCode(500, new { error = "An error occurred while processing your request" });
        }
    }
}
