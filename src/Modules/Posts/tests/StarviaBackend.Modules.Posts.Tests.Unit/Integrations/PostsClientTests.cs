using System.Net;
using FluentAssertions;
using StarviaBackend.Modules.Posts.Integrations;
using StarviaBackend.Modules.Posts.Tests.Unit.Fakes;

namespace StarviaBackend.Modules.Posts.Tests.Unit.Integrations;

public sealed class PostsClientTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _postId = Guid.NewGuid();

    private static PostsClient CreateClient(StubHttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("http://posts.test/") });

    private object PostBody(string? createdBy = null) => new
    {
        id = _postId,
        title = "Hello",
        createdAt = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc),
        createdBy = createdBy ?? _userId.ToString(),
    };

    private static object ErrorBody(string code) => new { errors = new[] { new { code, message = "boom" } } };

    [Fact]
    public async Task GetPostsAsync_ReturnsSummariesOfPostsOwnedByTheUser()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, PostBody());
        var client = CreateClient(handler);

        // Act
        var posts = await client.GetPostsAsync(_userId, [_postId]);

        // Assert
        var post = posts.Should().ContainSingle().Subject;
        post.Id.Should().Be(_postId);
        post.Title.Should().Be("Hello");
        post.CreatedAt.Should().Be(new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc));
        handler.RequestedUris.Should().ContainSingle().Which.AbsolutePath.Should().Be($"/v1/posts/{_postId}");
    }

    [Fact]
    public async Task GetPostsAsync_RequestsEachDistinctPostOnce()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, PostBody());
        var client = CreateClient(handler);

        // Act
        var posts = await client.GetPostsAsync(_userId, [_postId, _postId]);

        // Assert
        posts.Should().ContainSingle();
        handler.RequestedUris.Should().ContainSingle();
    }

    [Fact]
    public async Task GetPostsAsync_WithNoPosts_DoesNotCallTheApi()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, PostBody());
        var client = CreateClient(handler);

        // Act
        var posts = await client.GetPostsAsync(_userId, []);

        // Assert
        posts.Should().BeEmpty();
        handler.RequestedUris.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPostsAsync_WhenPayloadCreatorIsAnotherUser_ThrowsNotOwner()
    {
        // Arrange
        var client = CreateClient(StubHttpMessageHandler.Json(HttpStatusCode.OK, PostBody(createdBy: Guid.NewGuid().ToString())));

        // Act
        var act = () => client.GetPostsAsync(_userId, [_postId]);

        // Assert
        var exception = await act.Should().ThrowAsync<PostsIntegrationNotOwnerException>();
        exception.Which.Code.Should().Be("post_not_owned");
    }

    [Fact]
    public async Task GetPostsAsync_WhenPayloadHasNoCreator_ThrowsNotOwner()
    {
        // Arrange
        var body = new { id = _postId, title = "Hello", createdAt = DateTime.UtcNow, createdBy = (string?)null };
        var client = CreateClient(StubHttpMessageHandler.Json(HttpStatusCode.OK, body));

        // Act
        var act = () => client.GetPostsAsync(_userId, [_postId]);

        // Assert
        await act.Should().ThrowAsync<PostsIntegrationNotOwnerException>();
    }

    [Fact]
    public async Task GetPostsAsync_On404_ThrowsNotFound()
    {
        // Arrange
        var client = CreateClient(StubHttpMessageHandler.Text(HttpStatusCode.NotFound, string.Empty));

        // Act
        var act = () => client.GetPostsAsync(_userId, [_postId]);

        // Assert
        var exception = await act.Should().ThrowAsync<PostsIntegrationException>();
        exception.Which.Code.Should().Be("posts_integration_failed");
        exception.Which.Message.Should().Contain(_postId.ToString()).And.Contain("not found");
    }

    [Fact]
    public async Task GetPostsAsync_OnPostNotFoundErrorCode_ThrowsNotFound()
    {
        // Arrange
        var client = CreateClient(StubHttpMessageHandler.Json(HttpStatusCode.BadRequest, ErrorBody("post_not_found")));

        // Act
        var act = () => client.GetPostsAsync(_userId, [_postId]);

        // Assert
        var exception = await act.Should().ThrowAsync<PostsIntegrationException>();
        exception.Which.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task GetPostsAsync_OnPostNotOwnedErrorCode_ThrowsNotOwner()
    {
        // Arrange
        var client = CreateClient(StubHttpMessageHandler.Json(HttpStatusCode.BadRequest, ErrorBody("post_not_owned")));

        // Act
        var act = () => client.GetPostsAsync(_userId, [_postId]);

        // Assert
        await act.Should().ThrowAsync<PostsIntegrationNotOwnerException>();
    }

    [Fact]
    public async Task GetPostsAsync_On403_ThrowsNotOwner()
    {
        // Arrange
        var client = CreateClient(StubHttpMessageHandler.Text(HttpStatusCode.Forbidden, string.Empty));

        // Act
        var act = () => client.GetPostsAsync(_userId, [_postId]);

        // Assert
        await act.Should().ThrowAsync<PostsIntegrationNotOwnerException>();
    }

    [Fact]
    public async Task GetPostsAsync_OnUnknownServerError_ThrowsGenericFailure()
    {
        // Arrange
        var client = CreateClient(StubHttpMessageHandler.Text(HttpStatusCode.InternalServerError, "<html>oops</html>"));

        // Act
        var act = () => client.GetPostsAsync(_userId, [_postId]);

        // Assert
        var exception = await act.Should().ThrowAsync<PostsIntegrationException>();
        exception.Which.Message.Should().Be("Posts API returned 500.");
    }

    [Fact]
    public async Task GetPostsAsync_OnUnknownErrorCode_ThrowsGenericFailure()
    {
        // Arrange
        var client = CreateClient(StubHttpMessageHandler.Json(HttpStatusCode.BadRequest, ErrorBody("something_else")));

        // Act
        var act = () => client.GetPostsAsync(_userId, [_postId]);

        // Assert
        var exception = await act.Should().ThrowAsync<PostsIntegrationException>();
        exception.Which.Message.Should().Be("Posts API returned 400.");
    }

    [Fact]
    public async Task GetPostsAsync_WhenApiIsUnreachable_ThrowsUnreachable()
    {
        // Arrange
        var client = CreateClient(StubHttpMessageHandler.Throwing(new HttpRequestException("connection refused")));

        // Act
        var act = () => client.GetPostsAsync(_userId, [_postId]);

        // Assert
        var exception = await act.Should().ThrowAsync<PostsIntegrationException>();
        exception.Which.Message.Should().Contain("unreachable").And.Contain("connection refused");
    }

    [Fact]
    public async Task GetPostsAsync_WhenRequestTimesOut_ThrowsUnreachable()
    {
        // Arrange
        var client = CreateClient(StubHttpMessageHandler.Throwing(new TaskCanceledException("timeout")));

        // Act
        var act = () => client.GetPostsAsync(_userId, [_postId]);

        // Assert
        var exception = await act.Should().ThrowAsync<PostsIntegrationException>();
        exception.Which.Message.Should().Contain("unreachable");
    }

    [Fact]
    public async Task GetPostsAsync_WithEmptyUserId_Throws()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, PostBody());
        var client = CreateClient(handler);

        // Act
        var act = () => client.GetPostsAsync(Guid.Empty, [_postId]);

        // Assert
        await act.Should().ThrowAsync<PostsIntegrationException>().WithMessage("User id cannot be empty.");
        handler.RequestedUris.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPostsAsync_WithEmptyPostId_Throws()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, PostBody());
        var client = CreateClient(handler);

        // Act
        var act = () => client.GetPostsAsync(_userId, [Guid.Empty]);

        // Assert
        await act.Should().ThrowAsync<PostsIntegrationException>().WithMessage("Post id cannot be empty.");
        handler.RequestedUris.Should().BeEmpty();
    }
}
