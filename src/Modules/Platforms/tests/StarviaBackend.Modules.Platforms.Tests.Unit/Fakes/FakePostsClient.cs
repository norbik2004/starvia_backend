using StarviaBackend.Modules.Posts.Integrations;

namespace StarviaBackend.Modules.Platforms.Tests.Unit.Fakes;

internal sealed class FakePostsClient : IPostsClient
{
    public Dictionary<Guid, PostForPublish> Posts { get; } = new();
    public List<CreatePostPublicationRequest> CreatedPublications { get; } = [];

    public Task<IReadOnlyList<PostSummary>> GetPostsAsync(
        Guid userId,
        IEnumerable<Guid> postIds,
        CancellationToken cancellationToken = default)
    {
        var posts = postIds
            .Distinct()
            .Select(id => Posts.TryGetValue(id, out var post)
                ? new PostSummary(post.Id, post.Title, post.Body, [], DateTime.UtcNow)
                : throw new PostsIntegrationException($"Post '{id}' was not found in the posts API."))
            .ToList();

        return Task.FromResult<IReadOnlyList<PostSummary>>(posts);
    }

    public Task<PostForPublish> GetPostForPublishAsync(
        Guid userId,
        Guid postId,
        CancellationToken cancellationToken = default)
    {
        if (!Posts.TryGetValue(postId, out var post))
        {
            throw new PostsIntegrationException($"Post '{postId}' was not found in the posts API.");
        }

        return Task.FromResult(post);
    }

    public Task<PostPublicationRecord> CreatePublicationAsync(
        Guid userId,
        CreatePostPublicationRequest request,
        CancellationToken cancellationToken = default)
    {
        CreatedPublications.Add(request);
        return Task.FromResult(new PostPublicationRecord(
            Guid.NewGuid(),
            request.PostId,
            request.ExternalPostId,
            request.PublishedAt));
    }

    public Guid Seed(
        string body = "Hello LinkedIn",
        IReadOnlyList<PostAttachmentRef>? attachments = null,
        string title = "Title")
    {
        var id = Guid.NewGuid();
        Posts[id] = new PostForPublish(id, title, body, attachments ?? []);
        return id;
    }
}
