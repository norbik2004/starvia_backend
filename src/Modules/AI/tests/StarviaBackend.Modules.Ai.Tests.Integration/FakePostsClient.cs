using StarviaBackend.Modules.Posts.Integrations;

namespace StarviaBackend.Modules.Ai.Tests.Integration;

/// <summary>
/// Replaces the HTTP posts integration so the AI tests do not need the posts module.
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
            posts.Add(ResolveSummary(postId));
        }

        return Task.FromResult<IReadOnlyList<PostSummary>>(posts);
    }

    public Task<PostForPublish> GetPostForPublishAsync(
        Guid userId,
        Guid postId,
        CancellationToken cancellationToken = default)
    {
        var summary = ResolveSummary(postId);
        return Task.FromResult(new PostForPublish(summary.Id, summary.Title, summary.Body, []));
    }

    public Task<PostPublicationRecord> CreatePublicationAsync(
        Guid userId,
        CreatePostPublicationRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = ResolveSummary(request.PostId);
        return Task.FromResult(new PostPublicationRecord(
            Guid.NewGuid(),
            request.PostId,
            request.ExternalPostId,
            request.PublishedAt));
    }

    private PostSummary ResolveSummary(Guid postId)
    {
        if (UnknownPostIds.Contains(postId))
        {
            throw new PostsIntegrationException($"Post '{postId}' was not found in the posts API.");
        }

        if (ForeignPostIds.Contains(postId))
        {
            throw new PostsIntegrationNotOwnerException($"User is not the owner of post '{postId}'.");
        }

        return new PostSummary(
            postId,
            $"Post {postId}",
            $"Body of {postId}",
            [StarviaBackend.Shared.Abstractions.Platforms.PlatformType.LinkedIn],
            DateTime.UtcNow);
    }
}
