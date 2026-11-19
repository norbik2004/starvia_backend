using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Posts.Integrations;

/// <summary>
/// The port other modules depend on. Callers speak in these terms and never see the posts API's DTOs.
/// </summary>
public interface IPostsClient
{
    /// <summary>
    /// Loads the given posts on behalf of <paramref name="userId"/>. Throws <see cref="PostsIntegrationException"/>
    /// when a post does not exist and <see cref="PostsIntegrationNotOwnerException"/> when the user is not its creator.
    /// </summary>
    Task<IReadOnlyList<PostSummary>> GetPostsAsync(Guid userId, IEnumerable<Guid> postIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a post with ordered attachment file ids for platform publishing.
    /// Throws the same ownership / not-found exceptions as <see cref="GetPostsAsync"/>.
    /// </summary>
    Task<PostForPublish> GetPostForPublishAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a successful platform publish as a <c>PostPublication</c> owned by the Posts module.
    /// </summary>
    Task<PostPublicationRecord> CreatePublicationAsync(
        Guid userId,
        CreatePostPublicationRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>Domain-shaped post returned by the posts integration.</summary>
public sealed record PostSummary(
    Guid Id,
    string Title,
    string Body,
    IReadOnlyList<PlatformType> Platforms,
    DateTime CreatedAt);

/// <summary>Post payload needed to publish to an external platform.</summary>
public sealed record PostForPublish(
    Guid Id,
    string Title,
    string Body,
    IReadOnlyList<PostAttachmentRef> Attachments);

/// <summary>Ordered media file reference hanging off a post.</summary>
public sealed record PostAttachmentRef(Guid FileId, int Order);

/// <summary>Request to persist a publication after a successful external publish.</summary>
public sealed record CreatePostPublicationRequest(
    Guid PostId,
    Guid UserPlatformId,
    string ExternalPostId,
    DateTime PublishedAt);

/// <summary>Persisted publication returned by the posts integration.</summary>
public sealed record PostPublicationRecord(Guid Id, Guid PostId, string ExternalPostId, DateTime PublishedAt);
