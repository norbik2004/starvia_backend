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
}

/// <summary>Domain-shaped post returned by the posts integration.</summary>
public sealed record PostSummary(Guid Id, string Title, DateTime CreatedAt);
