using FluentValidation;

namespace StarviaBackend.Modules.Ai.Application.Chats.Commands.RemoveChat;

internal sealed class RemoveChatValidator : AbstractValidator<RemoveChatCommand>
{
    public RemoveChatValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty().WithMessage("Chat ID is required.");
        RuleFor(x => x.UserId).NotEmpty();
    }
}
