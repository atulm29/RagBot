using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RAGSERVERAPI.DTOs;
using RAGSERVERAPI.Services;

namespace RAGSERVERAPI.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ConversationController : ControllerBase
{
    private readonly IConversationService _conversationService;

    public ConversationController(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    [HttpPost]
    public async Task<ActionResult> CreateConversation([FromBody] CreateConversationRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var conversation = await _conversationService.CreateConversationAsync(request, userId);
        return Ok(conversation);
    }

    [HttpGet("{conversationId}")]
    public async Task<ActionResult<ConversationResponse>> GetConversation(Guid conversationId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var conversation = await _conversationService.GetConversationAsync(conversationId, userId);
            return Ok(conversation);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ConversationResponse>>> GetConversations()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var conversations = await _conversationService.GetUserConversationsAsync(userId);
        return Ok(conversations);
    }

    [HttpDelete("{conversationId}")]
    public async Task<ActionResult> DeleteConversation(Guid conversationId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _conversationService.DeleteConversationAsync(conversationId, userId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}
