using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace StarviaBackend.Modules.Posts.Tests.Integration.Posts;

public sealed class PostsEndpointsTests(PostsApp app) : PostsIntegrationTest(app)
{
    private const int DraftStatus = 1;
    private const int DoneStatus = 2;

    private static async Task UpdateStatusAsync(HttpClient client, Guid postId, string title, int status)
    {
        var response = await client.PutAsJsonAsync("/v1/posts", new { postId, title, body = "", status });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

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
    public async Task Get_post_of_another_user_returns_forbidden()
    {
        // Arrange
        var postId = await CreatePostAsync(CreateUserClient());
        var stranger = CreateUserClient();

        // Act
        var response = await stranger.GetAsync($"/v1/posts/{postId}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.Forbidden, "post_access_denied");
    }

    [Fact]
    public async Task Get_post_of_another_user_as_admin_returns_the_post()
    {
        // Arrange
        var postId = await CreatePostAsync(CreateUserClient());

        // Act
        var post = await GetPostAsync(CreateAdminClient(), postId);

        // Assert
        post.Id.Should().Be(postId);
    }

    [Fact]
    public async Task Get_post_returns_attachments_in_their_order()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        var attachmentIds = await AddAttachmentsAsync(client, postId, 3);
        var reorder = await client.PutAsJsonAsync("/v1/post-attachments/attachments/order", new
        {
            postId,
            postAttachmentId = attachmentIds[2],
            order = 0,
        });
        reorder.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act
        var post = await GetPostAsync(client, postId);

        // Assert
        post.PostAttachments.Select(a => a.Order).Should().Equal(0, 1, 2);
        post.PostAttachments.Select(a => a.Id).Should().Equal(attachmentIds[2], attachmentIds[0], attachmentIds[1]);
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
    public async Task Update_post_of_another_user_returns_forbidden()
    {
        // Arrange
        var owner = CreateUserClient();
        var postId = await CreatePostAsync(owner);

        // Act
        var response = await CreateUserClient().PutAsJsonAsync("/v1/posts", new
        {
            postId,
            title = UniqueTitle(),
            body = "",
            status = DraftStatus,
        });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.Forbidden, "post_access_denied");
    }

    [Fact]
    public async Task Update_post_of_another_user_as_admin_succeeds()
    {
        // Arrange
        var owner = CreateUserClient();
        var postId = await CreatePostAsync(owner);
        var newTitle = UniqueTitle();

        // Act
        var response = await CreateAdminClient().PutAsJsonAsync("/v1/posts", new
        {
            postId,
            title = newTitle,
            body = "Edited by admin",
            status = DoneStatus,
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var post = await GetPostAsync(owner, postId);
        post.Title.Should().Be(newTitle);
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
    public async Task Browse_posts_without_identity_returns_unauthorized()
    {
        // Act
        var response = await App.CreateClient().GetAsync("/v1/posts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Browse_posts_as_user_returns_only_their_own_posts()
    {
        // Arrange
        var owner = CreateUserClient();
        var mine = await CreatePostAsync(owner);
        var someoneElses = await CreatePostAsync(CreateUserClient());

        // Act
        var page = await owner.GetFromJsonAsync<PagedPostsResponse>("/v1/posts?pageSize=100");

        // Assert
        var ids = page!.Items.Select(p => p.Id).ToList();
        ids.Should().Contain(mine).And.NotContain(someoneElses);
        page.TotalCount.Should().Be(page.Items.Count);
    }

    [Fact]
    public async Task Browse_posts_as_admin_returns_everyones_posts()
    {
        // Arrange
        var first = await CreatePostAsync(CreateUserClient());
        var second = await CreatePostAsync(CreateUserClient());

        // Act
        var page = await CreateAdminClient().GetFromJsonAsync<PagedPostsResponse>("/v1/posts?pageSize=100");

        // Assert
        page!.Items.Select(p => p.Id).Should().Contain([first, second]);
    }

    [Fact]
    public async Task Browse_posts_filters_by_status_and_title_text()
    {
        // Arrange
        var client = CreateUserClient();
        var marker = Guid.NewGuid().ToString("N");
        var draft = await CreatePostAsync(client, $"Alpha {marker}");
        var done = await CreatePostAsync(client, $"Beta {marker}");
        await UpdateStatusAsync(client, done, $"Beta {marker}", DoneStatus);

        // Act
        var byText = await client.GetFromJsonAsync<PagedPostsResponse>($"/v1/posts?search={marker.ToUpperInvariant()}");
        var byStatus = await client.GetFromJsonAsync<PagedPostsResponse>($"/v1/posts?status={DoneStatus}");
        var byBoth = await client.GetFromJsonAsync<PagedPostsResponse>($"/v1/posts?status={DraftStatus}&search=alpha");

        // Assert
        byText!.Items.Select(p => p.Id).Should().BeEquivalentTo([draft, done]);
        byStatus!.Items.Select(p => p.Id).Should().Equal(done);
        byBoth!.Items.Select(p => p.Id).Should().Equal(draft);
    }

    [Fact]
    public async Task Browse_posts_treats_like_wildcards_in_search_literally()
    {
        // Arrange
        var client = CreateUserClient();
        await CreatePostAsync(client, "Plain title one");

        // Act
        var page = await client.GetFromJsonAsync<PagedPostsResponse>("/v1/posts?search=%25");

        // Assert
        page!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Browse_posts_finds_titles_containing_like_wildcard_characters_literally()
    {
        // Arrange
        var client = CreateUserClient();
        var literal = await CreatePostAsync(client, "100%_sure");
        await CreatePostAsync(client, "Plain title one");

        // Act
        var percent = await client.GetFromJsonAsync<PagedPostsResponse>("/v1/posts?search=%25");
        var underscore = await client.GetFromJsonAsync<PagedPostsResponse>("/v1/posts?search=_");

        // Assert
        percent!.Items.Select(p => p.Id).Should().Equal(literal);
        underscore!.Items.Select(p => p.Id).Should().Equal(literal);
    }

    [Fact]
    public async Task Browse_posts_sorts_by_title_and_reports_attachment_counts()
    {
        // Arrange
        var client = CreateUserClient();
        var b = await CreatePostAsync(client, "Bbb post");
        var a = await CreatePostAsync(client, "Aaa post");
        await AddAttachmentsAsync(client, b, 2);

        // Act
        var ascending = await client.GetFromJsonAsync<PagedPostsResponse>("/v1/posts?sortBy=Title&descending=false");
        var descending = await client.GetFromJsonAsync<PagedPostsResponse>("/v1/posts?sortBy=Title&descending=true");

        // Assert
        ascending!.Items.Select(p => p.Id).Should().Equal(a, b);
        descending!.Items.Select(p => p.Id).Should().Equal(b, a);
        ascending.Items.Single(p => p.Id == b).AttachmentsCount.Should().Be(2);
        ascending.Items.Single(p => p.Id == a).AttachmentsCount.Should().Be(0);
    }

    [Fact]
    public async Task Browse_posts_beyond_the_last_page_returns_no_items_but_the_total()
    {
        // Arrange
        var client = CreateUserClient();
        await CreatePostAsync(client);

        // Act
        var page = await client.GetFromJsonAsync<PagedPostsResponse>("/v1/posts?page=5&pageSize=10");

        // Assert
        page!.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(1);
        page.Page.Should().Be(5);
    }

    [Fact]
    public async Task Browse_posts_filtered_by_creator_as_admin_returns_only_that_users_posts()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var mine = await CreatePostAsync(CreateUserClient(ownerId));
        var other = await CreatePostAsync(CreateUserClient());

        // Act
        var page = await CreateAdminClient().GetFromJsonAsync<PagedPostsResponse>($"/v1/posts?createdBy={ownerId}&pageSize=100");

        // Assert
        var ids = page!.Items.Select(p => p.Id).ToList();
        ids.Should().Contain(mine).And.NotContain(other);
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
    public async Task Remove_post_of_another_user_returns_forbidden_and_keeps_it()
    {
        // Arrange
        var owner = CreateUserClient();
        var postId = await CreatePostAsync(owner);

        // Act
        var response = await DeleteWithBodyAsync(CreateUserClient(), "/v1/posts", new { postId });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.Forbidden, "post_access_denied");
        (await GetPostAsync(owner, postId)).Id.Should().Be(postId);
    }

    [Fact]
    public async Task Remove_post_of_another_user_as_admin_deletes_it()
    {
        // Arrange
        var owner = CreateUserClient();
        var postId = await CreatePostAsync(owner);

        // Act
        var response = await DeleteWithBodyAsync(CreateAdminClient(), "/v1/posts", new { postId });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var afterwards = await owner.GetAsync($"/v1/posts/{postId}");
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
