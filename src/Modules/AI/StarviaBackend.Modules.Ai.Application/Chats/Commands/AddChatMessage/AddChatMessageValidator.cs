using FluentValidation;

namespace StarviaBackend.Modules.Ai.Application.Chats.Commands.AddChatMessage;

internal sealed class AddChatMessageValidator : AbstractValidator<AddChatMessageCommand>
{
    public AddChatMessageValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty().WithMessage("Chat ID is required.");
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Request.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(AddChatMessageRequest.MaxContentLength)
            .WithMessage($"Content cannot exceed {AddChatMessageRequest.MaxContentLength} characters.");
    }
}
