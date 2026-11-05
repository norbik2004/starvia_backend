using FluentValidation;

namespace StarviaBackend.Modules.Ai.Application.PostGeneration.Commands.GeneratePost;

internal sealed class GeneratePostValidator : AbstractValidator<GeneratePostCommand>
{
    public GeneratePostValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Request.PostId)
            .NotEmpty().WithMessage("Post ID cannot be empty.");

        RuleFor(x => x.Request.Prompt)
            .NotEmpty().WithMessage("Prompt is required.")
            .MaximumLength(GeneratePostRequest.MaxPromptLength)
            .WithMessage($"Prompt cannot exceed {GeneratePostRequest.MaxPromptLength} characters.");
    }
}
