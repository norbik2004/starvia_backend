using FluentValidation;
using StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.AddPostAttachment;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;

namespace StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.AddPostAttachments;

internal sealed class AddPostAttachmentsValidator : AbstractValidator<AddPostAttachmentsCommand>
{
    public AddPostAttachmentsValidator()
    {
        RuleFor(x => x.request).NotNull();
        RuleFor(x => x.request.postId).NotEmpty().WithMessage("PostId cannot be empty.");
        RuleFor(x => x.userId).NotEmpty().WithMessage("UserId cannot be empty.");
        RuleFor(x => x.request.fileIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("At least one attachment is required.")
            .Must(fileIds => fileIds.Count <= PostAttachment.MaxPerPost)
            .WithMessage($"Maximum {PostAttachment.MaxPerPost} attachments are allowed per post.")
            .Must(fileIds => fileIds.TrueForAll(fileId => fileId != Guid.Empty))
            .WithMessage("FileId cannot be empty.");
    }
}
