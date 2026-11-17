using FluentValidation;

namespace StarviaBackend.Modules.Posts.Application.Posts.Commands.AddPost;

internal sealed class AddPostValidator : AbstractValidator<AddPostCommand>
{
    public AddPostValidator()
    {
        RuleFor(x => x.request.Title).NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);
        RuleFor(x => x.request.Platforms)
            .NotNull()
            .NotEmpty();
        RuleForEach(x => x.request.Platforms).IsInEnum();
        RuleFor(x => x.request.Platforms)
            .Must(p => p.Distinct().Count() == p.Count)
            .WithMessage("Platform types must be unique.")
            .When(x => x.request.Platforms is not null);
    }
}
