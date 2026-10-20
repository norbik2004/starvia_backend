using StarviaBackend.Modules.Posts.Integrations;

namespace StarviaBackend.Modules.Calendar.Tests.Unit.Fakes;

/// <summary>Stand-in for the posts integration: by default every requested post exists and belongs to the caller.</summary>
internal sealed class FakePostsClient : IPostsClient
{
    public HashSet<Guid> UnknownPostIds { get; } = [];
    public HashSet<Guid> ForeignPostIds { get; } = [];

    public List<(Guid UserId, Guid[] PostIds)> Calls { get; } = [];

    public Task<IReadOnlyList<PostSummary>> GetPostsAsync(
        Guid userId,
        IEnumerable<Guid> postIds,
        CancellationToken cancellationToken = default)
    {
        var requested = postIds.ToArray();
        Calls.Add((userId, requested));

        var posts = new List<PostSummary>();
        foreach (var postId in requested)
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
