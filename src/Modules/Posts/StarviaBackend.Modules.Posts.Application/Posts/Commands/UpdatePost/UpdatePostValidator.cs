using FluentValidation;
using StarviaBackend.Shared.Abstractions.Posts;

namespace StarviaBackend.Modules.Posts.Application.Posts.Commands.UpdatePost;

internal sealed class UpdatePostValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostValidator()
    {
        RuleFor(x => x.request.Title).NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);
        RuleFor(x => x.request.Body)
            .MinimumLength(0)
            .MaximumLength(PostLimits.MaxBodyLength);
        RuleFor(x => x.request.Status).IsInEnum();
    }
}
