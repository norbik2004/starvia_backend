using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Posts.Tests.Integration.Posts;

public sealed class PostPublicationsEndpointsTests(PostsApp app) : PostsIntegrationTest(app)
{
    private const int PublishedStatus = (int)PostPublicationStatus.Published;
    private const int DraftStatus = (int)PostPublicationStatus.Draft;

    // --- browse ------------------------------------------------------------------------------

    [Fact]
    public async Task Browse_publications_returns_them_newest_first()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        var older = await SeedPublicationAsync(postId);
        await Task.Delay(20);
        var newer = await SeedPublicationAsync(postId);

        // Act
        var publications = await client.GetFromJsonAsync<List<PostPublicationResponse>>($"/v1/post-publications?postId={postId}");

        // Assert
        publications!.Select(p => p.Id).Should().Equal(newer, older);
        publications.Should().OnlyContain(p => p.PostId == postId);
    }

    [Fact]
    public async Task Browse_publications_only_returns_publications_of_the_requested_post()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        var otherPostId = await CreatePostAsync(client);
        var mine = await SeedPublicationAsync(postId);
        await SeedPublicationAsync(otherPostId);

        // Act
        var publications = await client.GetFromJsonAsync<List<PostPublicationResponse>>($"/v1/post-publications?postId={postId}");

        // Assert
        publications!.Select(p => p.Id).Should().Equal(mine);
    }

    [Fact]
    public async Task Browse_publications_filters_by_status_and_user_platform()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        var platformId = Guid.NewGuid();
        var published = await SeedPublicationAsync(postId, PostPublicationStatus.Published, platformId);
        var draft = await SeedPublicationAsync(postId, PostPublicationStatus.Draft);

        // Act
        var byStatus = await client.GetFromJsonAsync<List<PostPublicationResponse>>(
            $"/v1/post-publications?postId={postId}&status={DraftStatus}");
        var byPlatform = await client.GetFromJsonAsync<List<PostPublicationResponse>>(
            $"/v1/post-publications?postId={postId}&userPlatformId={platformId}");

        // Assert
        byStatus!.Select(p => p.Id).Should().Equal(draft);
        byPlatform!.Select(p => p.Id).Should().Equal(published);
    }

    [Fact]
    public async Task Browse_publications_of_a_post_without_publications_returns_an_empty_list()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);

        // Act
        var publications = await client.GetFromJsonAsync<List<PostPublicationResponse>>($"/v1/post-publications?postId={postId}");

        // Assert
        publications.Should().BeEmpty();
    }

    [Fact]
    public async Task Browse_publications_of_another_users_post_returns_forbidden()
    {
        // Arrange
        var postId = await CreatePostAsync(CreateUserClient());
        await SeedPublicationAsync(postId);

        // Act
        var response = await CreateUserClient().GetAsync($"/v1/post-publications?postId={postId}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.Forbidden, "post_access_denied");
    }

    [Fact]
    public async Task Browse_publications_of_another_users_post_as_admin_succeeds()
    {
        // Arrange
        var postId = await CreatePostAsync(CreateUserClient());
        var publicationId = await SeedPublicationAsync(postId);

        // Act
        var publications = await CreateAdminClient().GetFromJsonAsync<List<PostPublicationResponse>>(
            $"/v1/post-publications?postId={postId}");

        // Assert
        publications!.Select(p => p.Id).Should().Equal(publicationId);
    }

    [Fact]
    public async Task Browse_publications_of_unknown_post_returns_post_not_found()
    {
        // Act
        var response = await Client.GetAsync($"/v1/post-publications?postId={Guid.NewGuid()}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "post_not_found");
    }

    [Fact]
    public async Task Browse_publications_without_identity_returns_unauthorized()
    {
        // Act
        var response = await App.CreateClient().GetAsync($"/v1/post-publications?postId={Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- record ------------------------------------------------------------------------------

    [Fact]
    public async Task Record_publication_persists_published_entry()
    {
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        var userPlatformId = Guid.NewGuid();
        var publishedAt = DateTime.UtcNow;

        var response = await client.PostAsJsonAsync(
            "/v1/post-publications",
            new
            {
                postId,
                userPlatformId,
                externalPostId = "urn:li:share:abc",
                publishedAt,
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var payload = await response.Content.ReadFromJsonAsync<PostPublicationResponse>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.PostId.Should().Be(postId);
        payload.UserPlatformId.Should().Be(userPlatformId);
        payload.ExternalPostId.Should().Be("urn:li:share:abc");
        payload.Status.Should().Be(PublishedStatus);

        var stored = await WithDbAsync<PostWriteDbContext, PostPublication>(db =>
            db.Set<PostPublication>().SingleAsync(p => p.Id == payload.Id));
        stored.ExternalPostId.Should().Be("urn:li:share:abc");
        stored.Status.Should().Be(PostPublicationStatus.Published);
    }

    // --- get ---------------------------------------------------------------------------------

    [Fact]
    public async Task Get_publication_returns_it_to_the_post_owner()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        var publicationId = await SeedPublicationAsync(postId);

        // Act
        var publication = await client.GetFromJsonAsync<PostPublicationResponse>($"/v1/post-publications/{publicationId}");

        // Assert
        publication!.Id.Should().Be(publicationId);
        publication.PostId.Should().Be(postId);
        publication.Status.Should().Be(PublishedStatus);
        publication.PublishedAt.Should().NotBeNull();
        publication.ExternalPostId.Should().StartWith("external-");
    }

    [Fact]
    public async Task Get_publication_of_another_users_post_returns_forbidden_but_admin_can_read_it()
    {
        // Arrange
        var postId = await CreatePostAsync(CreateUserClient());
        var publicationId = await SeedPublicationAsync(postId);

        // Act
        var asStranger = await CreateUserClient().GetAsync($"/v1/post-publications/{publicationId}");
        var asAdmin = await CreateAdminClient().GetAsync($"/v1/post-publications/{publicationId}");

        // Assert
        await AssertErrorCodeAsync(asStranger, HttpStatusCode.Forbidden, "post_access_denied");
        asAdmin.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_unknown_publication_returns_post_publication_not_found()
    {
        // Act
        var response = await Client.GetAsync($"/v1/post-publications/{Guid.NewGuid()}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "post_publication_not_found");
    }

    // --- get post ----------------------------------------------------------------------------

    [Fact]
    public async Task Get_post_includes_its_publications_newest_first()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = await CreatePostAsync(client);
        var older = await SeedPublicationAsync(postId);
        await Task.Delay(20);
        var newer = await SeedPublicationAsync(postId);

        // Act
        var post = await GetPostAsync(client, postId);

        // Assert
        post.PostPublications.Select(p => p.Id).Should().Equal(newer, older);
    }
}
