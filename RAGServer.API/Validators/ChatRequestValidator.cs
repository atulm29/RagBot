
using RAGSERVERAPI.DTOs;
using RAGSERVERAPI.Models;
using FluentValidation;

namespace RAGSERVERAPI.Validators;
public class ChatRequestValidator : AbstractValidator<ChatRequest>
{
    public ChatRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(10000).WithMessage("Message must not exceed 10000 characters");

        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("Conversation ID is required");
    }
}
