using FluentValidation;
using StarviaBackend.Modules.Ai.Core.Chats.Entities;

namespace StarviaBackend.Modules.Ai.Application.Chats.Commands.AddChat;

internal sealed class AddChatValidator : AbstractValidator<AddChatCommand>
{
    public AddChatValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Request.Type)
            .IsInEnum().WithMessage("Chat type is not supported.");

        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(Chat.MaxTitleLength).WithMessage($"Title cannot exceed {Chat.MaxTitleLength} characters.");
    }
}
