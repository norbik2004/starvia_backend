using FluentValidation;

namespace StarviaBackend.Modules.Posts.Application.PostPublications.Commands.RecordPostPublication;

internal sealed class RecordPostPublicationValidator : AbstractValidator<RecordPostPublicationCommand>
{
    public RecordPostPublicationValidator()
    {
        RuleFor(x => x.Request.PostId).NotEmpty();
        RuleFor(x => x.Request.UserPlatformId).NotEmpty();
        RuleFor(x => x.Request.ExternalPostId).NotEmpty();
        RuleFor(x => x.Request.PublishedAt).NotEmpty();
    }
}
