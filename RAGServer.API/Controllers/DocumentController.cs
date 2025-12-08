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
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IValidator<UploadDocumentRequest> _uploadValidator;
    private readonly IDocumentChunkRepository _documentChunkRepository;
    public DocumentController(IDocumentService documentService, IValidator<UploadDocumentRequest> uploadValidator, IDocumentChunkRepository documentChunkRepository)
    {
        _documentService = documentService;
        _uploadValidator = uploadValidator;
        _documentChunkRepository = documentChunkRepository;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<DocumentUploadResponse>> UploadDocument([FromForm] IFormFile file, [FromForm] Guid tenantId, [FromForm] Guid roleId, [FromForm] bool isPublic = false)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided" });
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var request = new UploadDocumentRequest(
            FileName: file.FileName,
            ContentType: file.ContentType,
            FileContent: memoryStream.ToArray(),
            TenantId: tenantId,
            RoleId: roleId,
            IsPublic: isPublic
        );

        var validationResult = await _uploadValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _documentService.UploadDocumentAsync(request, userId);
        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<List<DocumentListResponse>>> GetDocuments([FromQuery] Guid? tenantId = null)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var documents = await _documentService.GetUserDocumentsAsync(userId, tenantId);
        return Ok(documents);
    }

    [HttpGet("{documentId}")]
    public async Task<ActionResult> GetDocument(Guid documentId)
    {
        var document = await _documentService.GetDocumentByIdAsync(documentId);
        if (document == null)
        {
            return NotFound();
        }

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (document.UserId != userId)
        {
            return Forbid();
        }

        return Ok(document);
    }

    [HttpDelete("{documentId}")]
    public async Task<ActionResult> DeleteDocument(Guid documentId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _documentService.DeleteDocumentAsync(documentId, userId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("process/{documentId}")]
    public async Task<IActionResult> ProcessDocument(Guid documentId)
    {
        var result = await _documentService.ProcessDocumentAsync(documentId);
        return Ok(result);
    }

    [HttpGet("{id}/chunks")]
    public async Task<IActionResult> GetDocumentChunks(Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User ID not found"));

            //_logger.LogInformation("Getting chunks for document {DocumentId}", id);

            // Verify user owns the document
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return NotFound(new { message = "Document not found" });
            }

            if (document.UserId != userId)
            {
                return Forbid();
            }

            // Get chunks from repository
            var chunks = await _documentChunkRepository.GetByDocumentIdAsync(id);

            var response = chunks.Select(c => new
            {
                id = c.Id,
                documentId = c.DocumentId,
                chunkIndex = c.ChunkIndex,
                content = c.Content,
                tokenCount = c.TokenCount,
                createdAt = c.CreatedAt,
                metadata = c.Metadata
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error getting chunks for document {DocumentId}", id);
            return StatusCode(500, new { message = "Error retrieving document chunks" });
        }
    }
}
