using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StarviaBackend.Shared.Abstractions.Storage;

namespace StarviaBackend.Modules.Media.Tests.Integration.MediaFiles;

/// <summary>Who may do what with a media file: the uploader and admins, nobody else.</summary>
public sealed class MediaAccessEndpointsTests(MediaApp app) : MediaIntegrationTest(app)
{
    private const int ImageType = 0;
    private const int VideoType = 1;

    // ---- get / download ---------------------------------------------------------------------

    [Fact]
    public async Task Get_file_of_another_user_as_admin_succeeds()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var added = await UploadAsync(CreateUserClient(ownerId));

        // Act
        var file = await GetFileAsync(CreateAdminClient(), added.FileId);

        // Assert
        file.Id.Should().Be(added.FileId);
        file.CreatedBy.Should().Be(ownerId.ToString());
    }

    [Fact]
    public async Task Download_file_of_another_user_returns_forbidden()
    {
        // Arrange
        var added = await UploadAsync(CreateUserClient());

        // Act
        var response = await CreateUserClient().GetAsync($"/v1/media-files/{added.FileId}/content");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.Forbidden, "media_access_denied");
    }

    [Fact]
    public async Task Download_file_of_another_user_as_admin_returns_the_content()
    {
        // Arrange
        var content = new byte[] { 9, 8, 7 };
        var added = await UploadAsync(CreateUserClient(), content: content);

        // Act
        var response = await CreateAdminClient().GetAsync($"/v1/media-files/{added.FileId}/content");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsByteArrayAsync()).Should().Equal(content);
    }

    // ---- update -----------------------------------------------------------------------------

    [Fact]
    public async Task Update_file_of_another_user_returns_forbidden_and_keeps_the_description()
    {
        // Arrange
        var owner = CreateUserClient();
        var added = await UploadAsync(owner, description: "Original");

        // Act
        var response = await CreateUserClient().PutAsJsonAsync("/v1/media-files", new { fileId = added.FileId, description = "Hacked" });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.Forbidden, "media_access_denied");
        (await GetFileAsync(owner, added.FileId)).Description.Should().Be("Original");
    }

    [Fact]
    public async Task Update_file_of_another_user_as_admin_succeeds()
    {
        // Arrange
        var owner = CreateUserClient();
        var added = await UploadAsync(owner, description: "Original");

        // Act
        var response = await CreateAdminClient().PutAsJsonAsync("/v1/media-files", new { fileId = added.FileId, description = "By admin" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await GetFileAsync(owner, added.FileId)).Description.Should().Be("By admin");
    }

    // ---- remove -----------------------------------------------------------------------------

    [Fact]
    public async Task Remove_file_of_another_user_returns_forbidden_and_keeps_everything()
    {
        // Arrange
        var owner = CreateUserClient();
        var added = await UploadAsync(owner);

        // Act
        var response = await DeleteWithBodyAsync(CreateUserClient(), "/v1/media-files", new { fileId = added.FileId });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.Forbidden, "media_access_denied");
        App.Storage.Contains(added.FilePath, BucketNames.UserUploads).Should().BeTrue();
        (await GetFileAsync(owner, added.FileId)).Id.Should().Be(added.FileId);
    }

    [Fact]
    public async Task Remove_file_of_another_user_as_admin_deletes_it()
    {
        // Arrange
        var owner = CreateUserClient();
        var added = await UploadAsync(owner);

        // Act
        var response = await DeleteWithBodyAsync(CreateAdminClient(), "/v1/media-files", new { fileId = added.FileId });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        App.Storage.Contains(added.FilePath, BucketNames.UserUploads).Should().BeFalse();
        var afterwards = await owner.GetAsync($"/v1/media-files/{added.FileId}");
        await AssertErrorCodeAsync(afterwards, HttpStatusCode.BadRequest, "media_not_found");
    }

    // ---- browse -----------------------------------------------------------------------------

    [Fact]
    public async Task Browse_files_as_admin_returns_everybodys_files()
    {
        // Arrange
        var first = await UploadAsync(CreateUserClient());
        var second = await UploadAsync(CreateUserClient());

        // Act
        var page = await CreateAdminClient().GetFromJsonAsync<PagedFilesResponse>("/v1/media-files?pageSize=100");

        // Assert
        page!.Items.Select(f => f.Id).Should().Contain([first.FileId, second.FileId]);
        page.TotalCount.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Browse_files_as_admin_can_narrow_down_to_one_uploader()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var mine = await UploadAsync(CreateUserClient(ownerId));
        await UploadAsync(CreateUserClient());

        // Act
        var page = await CreateAdminClient().GetFromJsonAsync<PagedFilesResponse>($"/v1/media-files?createdBy={ownerId}");

        // Assert
        page!.Items.Select(f => f.Id).Should().Equal(mine.FileId);
        page.Items.Single().CreatedBy.Should().Be(ownerId.ToString());
    }

    [Fact]
    public async Task Browse_files_by_someone_elses_creator_id_as_a_plain_user_returns_nothing()
    {
        // Arrange
        var otherId = Guid.NewGuid();
        await UploadAsync(CreateUserClient(otherId));

        // Act
        var page = await CreateUserClient().GetFromJsonAsync<PagedFilesResponse>($"/v1/media-files?createdBy={otherId}");

        // Assert
        page!.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Browse_files_filters_by_type()
    {
        // Arrange
        var client = CreateUserClient();
        var image = await UploadAsync(client, "photo.png", "image/png");
        var video = await UploadAsync(client, "clip.mp4", "video/mp4");

        // Act
        var images = await client.GetFromJsonAsync<PagedFilesResponse>($"/v1/media-files?type={ImageType}");
        var videos = await client.GetFromJsonAsync<PagedFilesResponse>("/v1/media-files?type=Video");

        // Assert
        images!.Items.Select(f => f.Id).Should().Equal(image.FileId);
        videos!.Items.Select(f => f.Id).Should().Equal(video.FileId);
        videos.Items.Single().Type.Should().Be(VideoType);
    }

    [Fact]
    public async Task Browse_files_filters_by_source()
    {
        // Arrange
        var client = CreateUserClient();
        await UploadAsync(client);

        // Act
        var user = await client.GetFromJsonAsync<PagedFilesResponse>("/v1/media-files?source=User");
        var other = await client.GetFromJsonAsync<PagedFilesResponse>("/v1/media-files?source=System");

        // Assert
        user!.TotalCount.Should().Be(1);
        other!.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Browse_files_searches_the_name_and_the_description_ignoring_case_and_wildcards()
    {
        // Arrange
        var client = CreateUserClient();
        var byName = await UploadAsync(client, "Summer-Holiday.png");
        var byDescription = await UploadAsync(client, "other.png", description: "from the HOLIDAY trip");
        await UploadAsync(client, "unrelated.png", description: "nothing");
        await UploadAsync(client, "done.png", description: "100%_done");

        // Act
        var holiday = await client.GetFromJsonAsync<PagedFilesResponse>("/v1/media-files?search=holiday");
        var wildcard = await client.GetFromJsonAsync<PagedFilesResponse>("/v1/media-files?search=%25");

        // Assert
        holiday!.Items.Select(f => f.Id).Should().BeEquivalentTo([byName.FileId, byDescription.FileId]);
        wildcard!.Items.Should().ContainSingle().Which.FileName.Should().Be("done.png");
    }

    [Fact]
    public async Task Browse_files_sorts_by_name()
    {
        // Arrange
        var client = CreateUserClient();
        var b = await UploadAsync(client, "b.png");
        var a = await UploadAsync(client, "a.png");
        var c = await UploadAsync(client, "c.png");

        // Act
        var ascending = await client.GetFromJsonAsync<PagedFilesResponse>("/v1/media-files?sortBy=FileName&descending=false");
        var descending = await client.GetFromJsonAsync<PagedFilesResponse>("/v1/media-files?sortBy=FileName&descending=true");

        // Assert
        ascending!.Items.Select(f => f.Id).Should().Equal(a.FileId, b.FileId, c.FileId);
        descending!.Items.Select(f => f.Id).Should().Equal(c.FileId, b.FileId, a.FileId);
    }

    [Fact]
    public async Task Browse_files_page_past_the_end_returns_empty_items_but_the_total()
    {
        // Arrange
        var client = CreateUserClient();
        await UploadAsync(client);

        // Act
        var page = await client.GetFromJsonAsync<PagedFilesResponse>("/v1/media-files?page=500");

        // Assert
        page!.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(1);
        page.Page.Should().Be(500);
    }
}
