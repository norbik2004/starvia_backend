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
        var response = await client.PostAsJsonAsync("/v1/posts/attachments", new { postId, fileIds });

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
        var response = await client.PostAsJsonAsync("/v1/posts/attachments", new { postId, fileIds });

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
        var response = await client.PostAsJsonAsync("/v1/posts/attachments", new
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
        var response = await client.PostAsJsonAsync("/v1/posts/attachments", new { postId, fileIds = Array.Empty<Guid>() });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Add_attachments_to_unknown_post_returns_post_not_found()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/v1/posts/attachments", new
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
        var response = await client.PostAsJsonAsync("/v1/posts/attachments", new { postId, fileIds = new[] { missingFile } });

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
        var response = await client.PostAsJsonAsync("/v1/posts/attachments", new { postId, fileIds = new[] { foreignFile } });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "media_file_not_owned");
        (await GetPostAsync(client, postId)).PostAttachments.Should().BeEmpty();
    }

    [Fact]
    public async Task Add_attachments_without_identity_returns_unauthorized()
    {
        // Arrange
        var anonymous = App.CreateClient();

        // Act
        var response = await anonymous.PostAsJsonAsync("/v1/posts/attachments", new
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
        var response = await client.PutAsJsonAsync("/v1/posts/attachments/order", new
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
        var response = await client.PutAsJsonAsync("/v1/posts/attachments/order", new
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
        var response = await client.PutAsJsonAsync("/v1/posts/attachments/order", new
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
        var response = await client.PutAsJsonAsync("/v1/posts/attachments/order", new
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
        var response = await DeleteWithBodyAsync(client, "/v1/posts/attachments", new
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
        var response = await DeleteWithBodyAsync(client, "/v1/posts/attachments", new
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
        var response = await DeleteWithBodyAsync(Client, "/v1/posts/attachments", new
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
