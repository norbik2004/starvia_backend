using StarviaBackend.Modules.Posts.Integrations;

namespace StarviaBackend.Modules.Calendar.Tests.Integration;

/// <summary>
/// Replaces the HTTP posts integration so the Calendar tests do not need the posts module.
/// By default every requested post exists and belongs to the caller.
/// </summary>
public sealed class FakePostsClient : IPostsClient
{
    /// <summary>Posts that "do not exist"; requesting them fails like the real client does.</summary>
    public HashSet<Guid> UnknownPostIds { get; } = [];

    /// <summary>Posts created by somebody else; requesting them fails like the real client does.</summary>
    public HashSet<Guid> ForeignPostIds { get; } = [];

    public void Reset()
    {
        UnknownPostIds.Clear();
        ForeignPostIds.Clear();
    }

    public Task<IReadOnlyList<PostSummary>> GetPostsAsync(
        Guid userId,
        IEnumerable<Guid> postIds,
        CancellationToken cancellationToken = default)
    {
        var posts = new List<PostSummary>();

        foreach (var postId in postIds.Distinct())
        {
            if (UnknownPostIds.Contains(postId))
            {
                throw new PostsIntegrationException($"Post '{postId}' was not found in the posts API.");
            }

            if (ForeignPostIds.Contains(postId))
            {
                throw new PostsIntegrationNotOwnerException($"User is not the owner of post '{postId}'.");
            }

            posts.Add(new PostSummary(postId, $"Post {postId}", DateTime.UtcNow));
        }

        return Task.FromResult<IReadOnlyList<PostSummary>>(posts);
    }
}
