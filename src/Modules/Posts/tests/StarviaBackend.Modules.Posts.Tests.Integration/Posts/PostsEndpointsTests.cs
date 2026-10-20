using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace StarviaBackend.Modules.Posts.Tests.Integration.Posts;

public sealed class PostsEndpointsTests(PostsApp app) : PostsIntegrationTest(app)
{
    private const int DraftStatus = 1;
    private const int DoneStatus = 2;

    // --- add ---------------------------------------------------------------------------------

    [Fact]
    public async Task Add_post_returns_created_and_stores_a_draft_owned_by_the_caller()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        var title = UniqueTitle();

        // Act
        var postId = await CreatePostAsync(client, title);

        // Assert
        var post = await GetPostAsync(client, postId);
        post.Id.Should().Be(postId);
        post.Title.Should().Be(title);
        post.Status.Should().Be(DraftStatus);
        post.CreatedBy.Should().Be(userId.ToString());
        post.PostAttachments.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public async Task Add_post_with_invalid_title_returns_bad_request(string title)
    {
        // Act
        var response = await Client.PostAsJsonAsync("/v1/posts", new { title });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Add_post_without_identity_returns_unauthorized()
    {
        // Arrange
        var anonymous = App.CreateClient();

        // Act
        var response = await anonymous.PostAsJsonAsync("/v1/posts", new { title = UniqueTitle() });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- get ---------------------------------------------------------------------------------

    [Fact]
    public async Task Get_post_of_another_user_returns_post_not_owned()
    {
        // Arrange
        var postId = await CreatePostAsync(CreateUserClient());
        var stranger = CreateUserClient();

        // Act
        var response = await stranger.GetAsync($"/v1/posts/{postId}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "post_not_owned");
    }

    [Fact]
    public async Task Get_unknown_post_returns_post_not_found()
    {
        // Act
        var response = await Client.GetAsync($"/v1/posts/{Guid.NewGuid()}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "post_not_found");
    }

    [Fact]
    public async Task Get_post_without_identity_returns_unauthorized()
    {
        // Arrange
        var anonymous = App.CreateClient();

        // Act
        var response = await anonymous.GetAsync($"/v1/posts/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- update ------------------------------------------------------------------------------

    [Fact]
    public async Task Update_post_changes_its_title_body_and_status()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        var newTitle = UniqueTitle();

        // Act
        var response = await client.PutAsJsonAsync("/v1/posts", new
        {
            postId,
            title = newTitle,
            body = "Fresh body",
            status = DoneStatus,
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PostIdResponse>();
        result!.PostId.Should().Be(postId);

        var post = await GetPostAsync(client, postId);
        post.Title.Should().Be(newTitle);
        post.Body.Should().Be("Fresh body");
        post.Status.Should().Be(DoneStatus);
        post.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_post_with_invalid_title_returns_bad_request()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);

        // Act
        var response = await client.PutAsJsonAsync("/v1/posts", new { postId, title = "ab", body = "", status = DraftStatus });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_unknown_post_returns_post_not_found()
    {
        // Act
        var response = await Client.PutAsJsonAsync("/v1/posts", new
        {
            postId = Guid.NewGuid(),
            title = UniqueTitle(),
            body = "",
            status = DraftStatus,
        });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "post_not_found");
    }

    // --- browse (admin) ----------------------------------------------------------------------

    [Fact]
    public async Task Browse_posts_requires_admin_role()
    {
        // Act
        var asUser = await CreateUserClient().GetAsync("/v1/posts");
        var asAdmin = await CreateAdminClient().GetAsync("/v1/posts");

        // Assert
        asUser.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        asAdmin.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Browse_posts_lists_newest_first_and_pages_the_results()
    {
        // Arrange
        var client = CreateUserClient();
        var older = await CreatePostAsync(client);
        await Task.Delay(20);
        var newer = await CreatePostAsync(client);
        var admin = CreateAdminClient();

        // Act
        var page = await admin.GetFromJsonAsync<PagedPostsResponse>("/v1/posts?page=1&pageSize=100");
        var firstPage = await admin.GetFromJsonAsync<PagedPostsResponse>("/v1/posts?page=1&pageSize=1");

        // Assert
        var ids = page!.Items.Select(p => p.Id).ToList();
        ids.Should().Contain([older, newer]);
        ids.IndexOf(newer).Should().BeLessThan(ids.IndexOf(older));

        firstPage!.Items.Should().ContainSingle();
        firstPage.PageSize.Should().Be(1);
        firstPage.TotalCount.Should().BeGreaterThanOrEqualTo(2);
    }

    // --- remove ------------------------------------------------------------------------------

    [Fact]
    public async Task Remove_post_deletes_it()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);

        // Act
        var response = await DeleteWithBodyAsync(client, "/v1/posts", new { postId });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var afterwards = await client.GetAsync($"/v1/posts/{postId}");
        await AssertErrorCodeAsync(afterwards, HttpStatusCode.BadRequest, "post_not_found");
    }

    [Fact]
    public async Task Remove_unknown_post_returns_post_not_found()
    {
        // Act
        var response = await DeleteWithBodyAsync(Client, "/v1/posts", new { postId = Guid.NewGuid() });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "post_not_found");
    }

    [Fact]
    public async Task Remove_post_with_empty_id_returns_bad_request()
    {
        // Act
        var response = await DeleteWithBodyAsync(Client, "/v1/posts", new { postId = Guid.Empty });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Remove_post_deletes_its_attachments_too()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        await AddAttachmentsAsync(client, postId, 2);

        // Act
        var response = await DeleteWithBodyAsync(client, "/v1/posts", new { postId });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
