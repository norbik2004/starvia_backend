using FluentValidation;
using StarviaBackend.Modules.Ai.Core.Chats.Entities;

namespace StarviaBackend.Modules.Ai.Application.AskAgent.Commands.AskAgent;

internal sealed class AskAgentValidator : AbstractValidator<AskAgentCommand>
{
    public AskAgentValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Request.Prompt)
            .NotEmpty().WithMessage("Prompt is required.")
            .MaximumLength(AskAgentRequest.MaxPromptLength)
            .WithMessage($"Prompt cannot exceed {AskAgentRequest.MaxPromptLength} characters.");

        RuleFor(x => x.Request.ChatId)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("Chat ID cannot be empty.");

        RuleFor(x => x.Request.Title)
            .MaximumLength(Chat.MaxTitleLength)
            .When(x => x.Request.Title is not null)
            .WithMessage($"Title cannot exceed {Chat.MaxTitleLength} characters.");

        RuleFor(x => x.Request.BrandProfileId)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("Brand profile ID cannot be empty.");
    }
}
