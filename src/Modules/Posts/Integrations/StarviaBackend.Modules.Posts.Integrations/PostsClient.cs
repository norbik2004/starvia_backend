using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using StarviaBackend.Modules.Posts.Integrations.External;
using StarviaBackend.Shared.Infrastructure.Integrations;
using StarviaBackend.Shared.Infrastructure.Serialization;

namespace StarviaBackend.Modules.Posts.Integrations;

/// <summary>
/// Typed HttpClient implementation of <see cref="IPostsClient"/>. Loads each post from the posts API,
/// rejects the call when the requesting user is not the creator of the post, maps the wire format into
/// domain records, and turns transport failures into <see cref="PostsIntegrationException"/>.
/// </summary>
internal sealed class PostsClient(HttpClient httpClient) : IPostsClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new PlatformTypeJsonConverter() },
    };

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

            var payload = await GetPostPayloadAsync(userId, postId, cancellationToken);
            posts.Add(new PostSummary(
                payload.Id,
                payload.Title,
                payload.Body,
                payload.Platforms ?? [],
                payload.CreatedAt));
        }

        return posts;
    }

    public async Task<PostForPublish> GetPostForPublishAsync(
        Guid userId,
        Guid postId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new PostsIntegrationException("User id cannot be empty.");
        }

        if (postId == Guid.Empty)
        {
            throw new PostsIntegrationException("Post id cannot be empty.");
        }

        var payload = await GetPostPayloadAsync(userId, postId, cancellationToken);
        var attachments = (payload.PostAttachments ?? [])
            .OrderBy(a => a.Order)
            .Select(a => new PostAttachmentRef(a.FileId, a.Order))
            .ToList();

        return new PostForPublish(payload.Id, payload.Title, payload.Body, attachments);
    }

    public async Task<PostPublicationRecord> CreatePublicationAsync(
        Guid userId,
        CreatePostPublicationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new PostsIntegrationException("User id cannot be empty.");
        }

        ArgumentNullException.ThrowIfNull(request);

        if (request.PostId == Guid.Empty)
        {
            throw new PostsIntegrationException("Post id cannot be empty.");
        }

        if (request.UserPlatformId == Guid.Empty)
        {
            throw new PostsIntegrationException("User platform id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.ExternalPostId))
        {
            throw new PostsIntegrationException("External post id cannot be empty.");
        }

        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                "v1/post-publications",
                new
                {
                    postId = request.PostId,
                    userPlatformId = request.UserPlatformId,
                    externalPostId = request.ExternalPostId,
                    publishedAt = request.PublishedAt,
                },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw await response.ToFailureAsync(FailureRules(request.PostId), cancellationToken);
            }

            var payload = await response.Content.ReadFromJsonAsync<PostPublicationApiResponse>(JsonOptions, cancellationToken)
                          ?? throw new PostsIntegrationException("Posts API returned an empty body.");

            if (payload.Id == Guid.Empty
                || string.IsNullOrWhiteSpace(payload.ExternalPostId)
                || payload.PublishedAt is null)
            {
                throw new PostsIntegrationException("Posts API returned an incomplete publication.");
            }

            return new PostPublicationRecord(
                payload.Id,
                payload.PostId,
                payload.ExternalPostId,
                payload.PublishedAt.Value);
        }
        catch (Exception ex) when (ex is HttpRequestException
                                   || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            throw new PostsIntegrationException($"Posts API unreachable: {ex.Message}");
        }
    }

    private async Task<PostApiResponse> GetPostPayloadAsync(Guid userId, Guid postId, CancellationToken cancellationToken)
    {
        using var response = await SendGetAsync($"v1/posts/{postId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await response.ToFailureAsync(FailureRules(postId), cancellationToken);
        }

        var payload = await response.Content.ReadFromJsonAsync<PostApiResponse>(JsonOptions, cancellationToken)
                      ?? throw new PostsIntegrationException("Posts API returned an empty body.");

        if (!IsOwner(payload.CreatedBy, userId))
        {
            throw NotOwner(postId);
        }

        return payload;
    }

    private static IntegrationFailureRules FailureRules(Guid postId) =>
        new IntegrationFailureRules(status => new PostsIntegrationException($"Posts API returned {(int)status}."))
            .OnStatus(HttpStatusCode.NotFound, () => PostNotFound(postId))
            .OnStatus(HttpStatusCode.Forbidden, () => NotOwner(postId))
            .OnCode("post_not_found", () => PostNotFound(postId))
            .OnCode("post_not_owned", () => NotOwner(postId))
            .OnCode("post_access_denied", () => NotOwner(postId));

    private static PostsIntegrationException PostNotFound(Guid postId) =>
        new($"Post '{postId}' was not found in the posts API.");

    private static PostsIntegrationNotOwnerException NotOwner(Guid postId) =>
        new($"User is not the owner of post '{postId}'.");

    private static bool IsOwner(string? createdBy, Guid userId) =>
        Guid.TryParse(createdBy, out var ownerId) && ownerId == userId;

    private async Task<HttpResponseMessage> SendGetAsync(string path, CancellationToken cancellationToken)
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
