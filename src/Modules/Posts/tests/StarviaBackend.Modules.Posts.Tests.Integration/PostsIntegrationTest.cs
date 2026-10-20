using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Exceptions;
using StarviaBackend.Shared.Tests.Integration;

namespace StarviaBackend.Modules.Posts.Tests.Integration;

/// <summary>
/// All Posts integration tests share one container (per collection) and run sequentially: the
/// Testcontainers connection string is passed via a process-wide env var, so parallel classes
/// would race. New test classes just derive this and belong to the same collection.
/// </summary>
[Collection(PostsCollection.Name)]
public abstract class PostsIntegrationTest : IntegrationTest<PostsApp>
{
    protected PostsIntegrationTest(PostsApp app) : base(app)
    {
        app.Media.Reset();
    }

    protected HttpClient CreateUserClient(Guid? userId = null) =>
        CreateClient(userId ?? Guid.NewGuid(), UserRoles.User);

    protected HttpClient CreateAdminClient() => CreateClient(Guid.NewGuid(), UserRoles.Admin);

    protected static string UniqueTitle() => $"Post {Guid.NewGuid():N}";

    protected static async Task<Guid> CreatePostAsync(HttpClient client, string? title = null)
    {
        var response = await client.PostAsJsonAsync("/v1/posts", new { title = title ?? UniqueTitle() });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<PostIdResponse>();
        return body!.PostId;
    }

    protected static async Task<PostResponse> GetPostAsync(HttpClient client, Guid postId)
    {
        var response = await client.GetAsync($"/v1/posts/{postId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<PostResponse>())!;
    }

    protected static async Task<List<Guid>> AddAttachmentsAsync(HttpClient client, Guid postId, int count)
    {
        var fileIds = Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToList();
        var response = await client.PostAsJsonAsync("/v1/posts/attachments", new { postId, fileIds });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<AttachmentIdsResponse>();
        return body!.PostAttachmentIds;
    }

    protected static Task<HttpResponseMessage> DeleteWithBodyAsync(HttpClient client, string url, object body) =>
        client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url) { Content = JsonContent.Create(body) });

    protected static async Task AssertErrorCodeAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        response.StatusCode.Should().Be(status);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Errors.Should().Contain(e => e.Code == code);
    }

    protected sealed record PostIdResponse(Guid PostId);

    protected sealed record AttachmentIdsResponse(List<Guid> PostAttachmentIds);

    protected sealed record PostAttachmentResponse(Guid Id, Guid FileId, DateTime CreatedAt, int Order);

    protected sealed record PostResponse(
        Guid Id,
        string Title,
        string Body,
        int Status,
        DateTime CreatedAt,
        string CreatedBy,
        DateTime? LastModifiedAt,
        List<PostAttachmentResponse> PostAttachments);

    protected sealed record PagedPostsResponse(List<PostSummaryResponse> Items, int Page, int PageSize, long TotalCount);

    protected sealed record PostSummaryResponse(Guid Id, string Title, DateTime CreatedAt, string CreatedBy);
}

[CollectionDefinition(PostsCollection.Name)]
public sealed class PostsCollection : ICollectionFixture<PostsApp>
{
    public const string Name = "PostsIntegration";
}
