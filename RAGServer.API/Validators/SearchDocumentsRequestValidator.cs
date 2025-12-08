using RAGSERVERAPI.DTOs;
using RAGSERVERAPI.Models;
using FluentValidation;

namespace RAGSERVERAPI.Validators;

public class SearchDocumentsRequestValidator : AbstractValidator<SearchDocumentsRequest>
{
    public SearchDocumentsRequestValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Search query is required")
            .MaximumLength(1000).WithMessage("Query must not exceed 1000 characters");

        RuleFor(x => x.TopK)
            .GreaterThan(0).WithMessage("TopK must be greater than 0")
            .LessThanOrEqualTo(20).WithMessage("TopK must not exceed 20");

        RuleFor(x => x.MinSimilarity)
            .GreaterThanOrEqualTo(0.0).WithMessage("MinSimilarity must be between 0 and 1")
            .LessThanOrEqualTo(1.0).WithMessage("MinSimilarity must be between 0 and 1");
    }
}
