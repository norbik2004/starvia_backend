using FluentValidation;
using StarviaBackend.Modules.Ai.Core.Chats.Entities;

namespace StarviaBackend.Modules.Ai.Application.Chats.Commands.UpdateChat;

internal sealed class UpdateChatValidator : AbstractValidator<UpdateChatCommand>
{
    public UpdateChatValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty().WithMessage("Chat ID is required.");
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(Chat.MaxTitleLength).WithMessage($"Title cannot exceed {Chat.MaxTitleLength} characters.");

        RuleFor(x => x.Request.Status)
            .IsInEnum().WithMessage("Chat status is not supported.");
    }
}
