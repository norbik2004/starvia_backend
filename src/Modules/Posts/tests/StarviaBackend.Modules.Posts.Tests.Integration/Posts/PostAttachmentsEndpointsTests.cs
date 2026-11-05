using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace StarviaBackend.Modules.Posts.Tests.Integration.Posts;

public sealed class PostAttachmentsEndpointsTests(PostsApp app) : PostsIntegrationTest(app)
{
    // --- add ---------------------------------------------------------------------------------

    [Fact]
    public async Task Add_attachments_stores_them_with_sequential_order()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        var fileIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var response = await client.PostAsJsonAsync("/v1/post-attachments/attachments", new { postId, fileIds });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await response.Content.ReadFromJsonAsync<AttachmentIdsResponse>())!.PostAttachmentIds;
        created.Should().HaveCount(3);

        var post = await GetPostAsync(client, postId);
        post.PostAttachments.Should().HaveCount(3);
        for (var index = 0; index < 3; index++)
        {
            var attachment = post.PostAttachments.Single(a => a.Id == created[index]);
            attachment.FileId.Should().Be(fileIds[index]);
            attachment.Order.Should().Be(index);
        }
    }

    [Fact]
    public async Task Add_attachments_continues_the_order_after_existing_ones()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        await AddAttachmentsAsync(client, postId, 2);

        // Act
        await AddAttachmentsAsync(client, postId, 2);

        // Assert
        var post = await GetPostAsync(client, postId);
        post.PostAttachments.Select(a => a.Order).Should().BeEquivalentTo([0, 1, 2, 3]);
    }

    [Fact]
    public async Task Add_more_attachments_than_allowed_in_one_request_returns_bad_request()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        var fileIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToList();

        // Act
        var response = await client.PostAsJsonAsync("/v1/post-attachments/attachments", new { postId, fileIds });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Add_attachments_beyond_the_limit_across_requests_returns_too_many_attachments()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        await AddAttachmentsAsync(client, postId, 4);

        // Act
        var response = await client.PostAsJsonAsync("/v1/post-attachments/attachments", new
        {
            postId,
            fileIds = new[] { Guid.NewGuid(), Guid.NewGuid() },
        });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "too_many_post_attachments");
        (await GetPostAsync(client, postId)).PostAttachments.Should().HaveCount(4);
    }

    [Fact]
    public async Task Add_attachments_without_files_returns_bad_request()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);

        // Act
        var response = await client.PostAsJsonAsync("/v1/post-attachments/attachments", new { postId, fileIds = Array.Empty<Guid>() });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Add_attachments_to_unknown_post_returns_post_not_found()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/v1/post-attachments/attachments", new
        {
            postId = Guid.NewGuid(),
            fileIds = new[] { Guid.NewGuid() },
        });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "post_not_found");
    }

    [Fact]
    public async Task Add_attachment_unknown_to_media_returns_media_integration_failure()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        var missingFile = Guid.NewGuid();
        App.Media.UnknownFileIds.Add(missingFile);

        // Act
        var response = await client.PostAsJsonAsync("/v1/post-attachments/attachments", new { postId, fileIds = new[] { missingFile } });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "media_integration_failed");
        (await GetPostAsync(client, postId)).PostAttachments.Should().BeEmpty();
    }

    [Fact]
    public async Task Add_attachment_owned_by_someone_else_returns_not_owner_error()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        var foreignFile = Guid.NewGuid();
        App.Media.ForeignFileIds.Add(foreignFile);

        // Act
        var response = await client.PostAsJsonAsync("/v1/post-attachments/attachments", new { postId, fileIds = new[] { foreignFile } });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "media_file_not_owned");
        (await GetPostAsync(client, postId)).PostAttachments.Should().BeEmpty();
    }

    // --- browse ------------------------------------------------------------------------------

    [Fact]
    public async Task Browse_attachments_returns_them_in_display_order()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        var ids = await AddAttachmentsAsync(client, postId, 3);
        var reorder = await client.PutAsJsonAsync("/v1/post-attachments/attachments/order", new
        {
            postId,
            postAttachmentId = ids[2],
            order = 0,
        });
        reorder.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act
        var attachments = await client.GetFromJsonAsync<List<PostAttachmentResponse>>($"/v1/post-attachments?postId={postId}");

        // Assert
        attachments!.Select(a => a.Id).Should().Equal(ids[2], ids[0], ids[1]);
        attachments.Select(a => a.Order).Should().Equal(0, 1, 2);
    }

    [Fact]
    public async Task Browse_attachments_of_a_post_without_attachments_returns_an_empty_list()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);

        // Act
        var attachments = await client.GetFromJsonAsync<List<PostAttachmentResponse>>($"/v1/post-attachments?postId={postId}");

        // Assert
        attachments.Should().BeEmpty();
    }

    [Fact]
    public async Task Browse_attachments_of_another_users_post_returns_forbidden_but_admin_can_read_them()
    {
        // Arrange
        var owner = CreateUserClient();
        var postId = await CreatePostAsync(owner);
        var ids = await AddAttachmentsAsync(owner, postId, 2);

        // Act
        var asStranger = await CreateUserClient().GetAsync($"/v1/post-attachments?postId={postId}");
        var asAdmin = await CreateAdminClient().GetFromJsonAsync<List<PostAttachmentResponse>>($"/v1/post-attachments?postId={postId}");

        // Assert
        await AssertErrorCodeAsync(asStranger, HttpStatusCode.Forbidden, "post_access_denied");
        asAdmin!.Select(a => a.Id).Should().Equal(ids);
    }

    [Fact]
    public async Task Browse_attachments_of_unknown_post_returns_post_not_found()
    {
        // Act
        var response = await Client.GetAsync($"/v1/post-attachments?postId={Guid.NewGuid()}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "post_not_found");
    }

    [Fact]
    public async Task Browse_attachments_without_identity_returns_unauthorized()
    {
        // Act
        var response = await App.CreateClient().GetAsync($"/v1/post-attachments?postId={Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Managing_attachments_of_another_users_post_returns_forbidden()
    {
        // Arrange
        var owner = CreateUserClient();
        var postId = await CreatePostAsync(owner);
        var attachmentId = (await AddAttachmentsAsync(owner, postId, 1))[0];
        var stranger = CreateUserClient();

        // Act
        var add = await stranger.PostAsJsonAsync("/v1/post-attachments/attachments", new { postId, fileIds = new[] { Guid.NewGuid() } });
        var reorder = await stranger.PutAsJsonAsync("/v1/post-attachments/attachments/order", new { postId, postAttachmentId = attachmentId, order = 0 });
        var remove = await DeleteWithBodyAsync(stranger, "/v1/post-attachments/attachments", new { postId, postAttachmentId = attachmentId });

        // Assert
        await AssertErrorCodeAsync(add, HttpStatusCode.Forbidden, "post_access_denied");
        await AssertErrorCodeAsync(reorder, HttpStatusCode.Forbidden, "post_access_denied");
        await AssertErrorCodeAsync(remove, HttpStatusCode.Forbidden, "post_access_denied");
        (await GetPostAsync(owner, postId)).PostAttachments.Should().ContainSingle();
    }

    [Fact]
    public async Task Admin_can_manage_attachments_of_another_users_post()
    {
        // Arrange
        var owner = CreateUserClient();
        var postId = await CreatePostAsync(owner);
        var admin = CreateAdminClient();

        // Act
        var added = await AddAttachmentsAsync(admin, postId, 2);
        var remove = await DeleteWithBodyAsync(admin, "/v1/post-attachments/attachments", new { postId, postAttachmentId = added[0] });

        // Assert
        remove.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await GetPostAsync(owner, postId)).PostAttachments.Should().ContainSingle().Which.Id.Should().Be(added[1]);
    }

    [Fact]
    public async Task Add_attachments_without_identity_returns_unauthorized()
    {
        // Arrange
        var anonymous = App.CreateClient();

        // Act
        var response = await anonymous.PostAsJsonAsync("/v1/post-attachments/attachments", new
        {
            postId = Guid.NewGuid(),
            fileIds = new[] { Guid.NewGuid() },
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- reorder -----------------------------------------------------------------------------

    [Fact]
    public async Task Update_attachment_order_moves_the_attachment_and_shifts_the_rest()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        var ids = await AddAttachmentsAsync(client, postId, 3);

        // Act
        var response = await client.PutAsJsonAsync("/v1/post-attachments/attachments/order", new
        {
            postId,
            postAttachmentId = ids[2],
            order = 0,
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var post = await GetPostAsync(client, postId);
        OrderOf(post, ids[2]).Should().Be(0);
        OrderOf(post, ids[0]).Should().Be(1);
        OrderOf(post, ids[1]).Should().Be(2);
    }

    [Fact]
    public async Task Update_attachment_order_out_of_range_returns_invalid_order()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        var ids = await AddAttachmentsAsync(client, postId, 2);

        // Act
        var response = await client.PutAsJsonAsync("/v1/post-attachments/attachments/order", new
        {
            postId,
            postAttachmentId = ids[0],
            order = 2,
        });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "invalid_post_attachment_order");
    }

    [Fact]
    public async Task Update_attachment_order_with_negative_order_returns_bad_request()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        var ids = await AddAttachmentsAsync(client, postId, 2);

        // Act
        var response = await client.PutAsJsonAsync("/v1/post-attachments/attachments/order", new
        {
            postId,
            postAttachmentId = ids[0],
            order = -1,
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_attachment_order_of_unknown_attachment_returns_not_found_code()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        await AddAttachmentsAsync(client, postId, 2);

        // Act
        var response = await client.PutAsJsonAsync("/v1/post-attachments/attachments/order", new
        {
            postId,
            postAttachmentId = Guid.NewGuid(),
            order = 0,
        });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "post_attachment_not_found");
    }

    // --- remove ------------------------------------------------------------------------------

    [Fact]
    public async Task Remove_attachment_deletes_it_and_closes_the_gap_in_the_order()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        var ids = await AddAttachmentsAsync(client, postId, 3);

        // Act
        var response = await DeleteWithBodyAsync(client, "/v1/post-attachments/attachments", new
        {
            postId,
            postAttachmentId = ids[1],
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var post = await GetPostAsync(client, postId);
        post.PostAttachments.Select(a => a.Id).Should().BeEquivalentTo([ids[0], ids[2]]);
        OrderOf(post, ids[0]).Should().Be(0);
        OrderOf(post, ids[2]).Should().Be(1);
    }

    [Fact]
    public async Task Remove_unknown_attachment_returns_not_found_code()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);

        // Act
        var response = await DeleteWithBodyAsync(client, "/v1/post-attachments/attachments", new
        {
            postId,
            postAttachmentId = Guid.NewGuid(),
        });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "post_attachment_not_found");
    }

    [Fact]
    public async Task Remove_attachment_of_unknown_post_returns_post_not_found()
    {
        // Act
        var response = await DeleteWithBodyAsync(Client, "/v1/post-attachments/attachments", new
        {
            postId = Guid.NewGuid(),
            postAttachmentId = Guid.NewGuid(),
        });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "post_not_found");
    }

    private static int OrderOf(PostResponse post, Guid attachmentId) =>
        post.PostAttachments.Single(a => a.Id == attachmentId).Order;
}
