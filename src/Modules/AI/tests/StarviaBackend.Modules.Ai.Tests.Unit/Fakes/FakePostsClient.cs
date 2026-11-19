using StarviaBackend.Modules.Posts.Integrations;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Ai.Tests.Unit.Fakes;

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

        return new PostSummary(postId, $"Post {postId}", $"Body of {postId}", [PlatformType.LinkedIn], DateTime.UtcNow);
    }
}
