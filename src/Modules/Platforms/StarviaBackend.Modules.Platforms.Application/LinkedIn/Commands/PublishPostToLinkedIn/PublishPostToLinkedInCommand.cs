using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Platforms.Application.LinkedIn.Commands.PublishPostToLinkedIn;

public sealed record PublishPostToLinkedInCommand(PublishPostToLinkedInRequest Request, Guid UserId)
    : ICommand<PublishPostToLinkedInResult>;

public sealed record PublishPostToLinkedInRequest(
    Guid PostId,
    Guid UserPlatformId,
    string? ArticleUrl = null,
    string? ArticleDescription = null);

public sealed record PublishPostToLinkedInResult(
    Guid PostPublicationId,
    string ExternalPostId,
    DateTime PublishedAt);
