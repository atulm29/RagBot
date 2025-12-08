
using RAGSERVERAPI.DTOs;
using RAGSERVERAPI.Models;
using FluentValidation;

namespace RAGSERVERAPI.Validators;
public class UploadDocumentRequestValidator : AbstractValidator<UploadDocumentRequest>
{
    private static readonly string[] AllowedContentTypes =
    {
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/msword",
        "text/plain"
    };

    public UploadDocumentRequestValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .MaximumLength(500).WithMessage("File name must not exceed 500 characters");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required")
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage("Only PDF, DOCX, DOC, and TXT files are allowed");

        RuleFor(x => x.FileContent)
            .NotEmpty().WithMessage("File content is required")
            .Must(content => content.Length <= 50 * 1024 * 1024)
            .WithMessage("File size must not exceed 50MB");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required");
    }
}
