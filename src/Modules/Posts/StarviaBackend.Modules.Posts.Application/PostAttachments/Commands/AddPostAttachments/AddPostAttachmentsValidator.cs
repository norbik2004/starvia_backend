using FluentValidation;
using StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.AddPostAttachment;

namespace StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.AddPostAttachments;

internal sealed class AddPostAttachmentsValidator : AbstractValidator<AddPostAttachmentsCommand>
{
    public AddPostAttachmentsValidator()
    {
        RuleFor(x => x.request).NotNull();
        RuleFor(x => x.request.postId).NotEmpty().WithMessage("PostId cannot be empty.");
        RuleFor(x => x.request.order).GreaterThanOrEqualTo(0).WithMessage("Order must be greater than or equal to 0.");
        RuleFor(x => x.userId).NotEmpty().WithMessage("UserId cannot be empty.");
        RuleFor(x => x.request.fileIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("At least one attachment is required.")
            .Must(fileIds => fileIds.Count <= 5)
            .WithMessage("Maximum 5 attachments are allowed per post.")
            .Must(fileIds => fileIds.TrueForAll(fileId => fileId != Guid.Empty))
            .WithMessage("FileId cannot be empty.");
    }
}
