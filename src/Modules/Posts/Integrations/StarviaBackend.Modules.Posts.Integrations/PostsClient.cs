using System.Net;
using System.Net.Http.Json;
using StarviaBackend.Modules.Posts.Integrations.External;
using StarviaBackend.Shared.Infrastructure.Integrations;

namespace StarviaBackend.Modules.Posts.Integrations;

/// <summary>
/// Typed HttpClient implementation of <see cref="IPostsClient"/>. Loads each post from the posts API,
/// rejects the call when the requesting user is not the creator of the post, maps the wire format into
/// <see cref="PostSummary"/>, and turns transport failures into <see cref="PostsIntegrationException"/>.
/// </summary>
internal sealed class PostsClient(HttpClient httpClient) : IPostsClient
{
    public async Task<IReadOnlyList<PostSummary>> GetPostsAsync(
        Guid userId,
        IEnumerable<Guid> postIds,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new PostsIntegrationException("User id cannot be empty.");
        }

        var posts = new List<PostSummary>();

        foreach (var postId in postIds.Distinct())
        {
            if (postId == Guid.Empty)
            {
                throw new PostsIntegrationException("Post id cannot be empty.");
            }

            posts.Add(await GetPostAsync(userId, postId, cancellationToken));
        }

        return posts;
    }

    private async Task<PostSummary> GetPostAsync(Guid userId, Guid postId, CancellationToken cancellationToken)
    {
        using var response = await SendAsync($"v1/posts/{postId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await response.ToFailureAsync(FailureRules(postId), cancellationToken);
        }

        var payload = await response.Content.ReadFromJsonAsync<PostApiResponse>(cancellationToken)
                      ?? throw new PostsIntegrationException("Posts API returned an empty body.");

        if (!IsOwner(payload.CreatedBy, userId))
        {
            throw NotOwner(postId);
        }

        return new PostSummary(payload.Id, payload.Title, payload.CreatedAt);
    }

    private static IntegrationFailureRules FailureRules(Guid postId) =>
        new IntegrationFailureRules(status => new PostsIntegrationException($"Posts API returned {(int)status}."))
            .OnStatus(HttpStatusCode.NotFound, () => PostNotFound(postId))
            .OnStatus(HttpStatusCode.Forbidden, () => NotOwner(postId))
            .OnCode("post_not_found", () => PostNotFound(postId))
            .OnCode("post_not_owned", () => NotOwner(postId));

    private static PostsIntegrationException PostNotFound(Guid postId) =>
        new($"Post '{postId}' was not found in the posts API.");

    private static PostsIntegrationNotOwnerException NotOwner(Guid postId) =>
        new($"User is not the owner of post '{postId}'.");

    private static bool IsOwner(string? createdBy, Guid userId) =>
        Guid.TryParse(createdBy, out var ownerId) && ownerId == userId;

    private async Task<HttpResponseMessage> SendAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.GetAsync(path, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException
                                   || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            throw new PostsIntegrationException($"Posts API unreachable: {ex.Message}");
        }
    }
}
