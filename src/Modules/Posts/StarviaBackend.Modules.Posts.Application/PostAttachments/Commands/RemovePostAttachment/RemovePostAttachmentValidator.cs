using FluentValidation;

namespace StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.RemovePostAttachment;

internal sealed class RemovePostAttachmentValidator : AbstractValidator<RemovePostAttachmentCommand>
{
    public RemovePostAttachmentValidator()
    {
        RuleFor(x => x.request).NotNull();
        RuleFor(x => x.request.postId).NotEmpty().WithMessage("PostId cannot be empty.");
        RuleFor(x => x.request.postAttachmentId).NotEmpty().WithMessage("PostAttachmentId cannot be empty.");
        RuleFor(x => x.userId).NotEmpty().WithMessage("UserId cannot be empty.");
    }
}
