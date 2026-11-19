using FluentValidation;

namespace StarviaBackend.Modules.Platforms.Application.LinkedIn.Commands.PublishPostToLinkedIn;

internal sealed class PublishPostToLinkedInValidator : AbstractValidator<PublishPostToLinkedInCommand>
{
    public PublishPostToLinkedInValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Request.PostId).NotEmpty();
        RuleFor(x => x.Request.UserPlatformId).NotEmpty();
        RuleFor(x => x.Request.ArticleUrl)
            .Must(BeAbsoluteHttpUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.Request.ArticleUrl))
            .WithMessage("ArticleUrl must be an absolute http(s) URL.");
    }

    private static bool BeAbsoluteHttpUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
