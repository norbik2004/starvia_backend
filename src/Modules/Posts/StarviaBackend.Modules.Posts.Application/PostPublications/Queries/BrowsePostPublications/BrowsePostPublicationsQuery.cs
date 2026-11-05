using StarviaBackend.Modules.Posts.Core.Posts.Enums;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Posts.Application.PostPublications.Queries.BrowsePostPublications;

/// <summary>
/// Lists the publications of one post, newest first. The caller needs to be allowed to view
/// the post's publications (see the posts access policy).
/// </summary>
/// <param name="Status">Only publications in this status.</param>
/// <param name="UserPlatformId">Only publications made to this user platform.</param>
internal sealed record BrowsePostPublicationsQuery(
    Guid PostId,
    PostPublicationStatus? Status = null,
    Guid? UserPlatformId = null) : IQuery<IReadOnlyList<PostPublicationDto>>;

public sealed record PostPublicationDto(
    Guid Id,
    Guid PostId,
    PostPublicationStatus Status,
    Guid UserPlatformId,
    DateTime? PublishedAt,
    string? ExternalPostId,
    DateTime CreatedAt);
