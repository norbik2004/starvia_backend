using FluentValidation;

namespace StarviaBackend.Modules.Ai.Application.PostGeneration.Commands.GeneratePost;

internal sealed class GeneratePostValidator : AbstractValidator<GeneratePostCommand>
{
    public GeneratePostValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Request.ChatId)
            .NotEqual(Guid.Empty).WithMessage("Chat ID cannot be empty.")
            .When(x => x.Request.ChatId.HasValue);

        RuleFor(x => x.Request.Prompt)
            .NotEmpty().WithMessage("Prompt is required.")
            .MaximumLength(GeneratePostRequest.MaxPromptLength)
            .WithMessage($"Prompt cannot exceed {GeneratePostRequest.MaxPromptLength} characters.");
    }
}
