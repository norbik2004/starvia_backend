using FluentValidation;

namespace StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.UpdatePostAttachmentOrder;

internal sealed class UpdatePostAttachmentOrderValidator : AbstractValidator<UpdatePostAttachmentOrderCommand>
{
    public UpdatePostAttachmentOrderValidator()
    {
        RuleFor(x => x.request).NotNull();
        RuleFor(x => x.request.postId).NotEmpty().WithMessage("PostId cannot be empty.");
        RuleFor(x => x.request.postAttachmentId).NotEmpty().WithMessage("PostAttachmentId cannot be empty.");
        RuleFor(x => x.request.order).GreaterThanOrEqualTo(0).WithMessage("Order must be greater than or equal to 0.");
        RuleFor(x => x.userId).NotEmpty().WithMessage("UserId cannot be empty.");
    }
}
