using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StarviaBackend.Shared.Abstractions.Storage;

namespace StarviaBackend.Modules.Media.Tests.Integration.MediaFiles;

public sealed class MediaFilesEndpointsTests(MediaApp app) : MediaIntegrationTest(app)
{
    private const int UserSource = 0;

    // --- upload ------------------------------------------------------------------------------

    [Fact]
    public async Task Upload_file_stores_the_content_and_the_metadata()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        var content = new byte[] { 10, 20, 30, 40, 50 };

        // Act
        var added = await UploadAsync(client, "photo.png", "image/png", content, "Team photo");

        // Assert
        added.FileName.Should().Be("photo.png");
        App.Storage.Read(added.FilePath, BucketNames.UserUploads).Should().Equal(content);

        var file = await GetFileAsync(client, added.FileId);
        file.Id.Should().Be(added.FileId);
        file.FileName.Should().Be("photo.png");
        file.Description.Should().Be("Team photo");
        file.Source.Should().Be(UserSource);
        file.CreatedBy.Should().Be(userId.ToString());
        file.Url.Should().Be($"/v1/media-files/{added.FileId}/content");
    }

    [Fact]
    public async Task Upload_file_puts_it_under_the_callers_email()
    {
        // Arrange
        var client = CreateUserClient(email: "jane@example.com");

        // Act
        var added = await UploadAsync(client, "report.pdf", "application/pdf");

        // Assert
        added.FilePath.Should().StartWith("jane@example.com/report_").And.EndWith(".pdf");
    }

    [Theory]
    [InlineData("photo.png", "image/png", 0)]
    [InlineData("clip.mp4", "video/mp4", 1)]
    [InlineData("song.mp3", "audio/mpeg", 2)]
    [InlineData("report.pdf", "application/pdf", 3)]
    [InlineData("archive.zip", "application/zip", 4)]
    public async Task Upload_file_resolves_the_file_type_from_the_content_type(string fileName, string contentType, int expectedType)
    {
        // Arrange
        var client = CreateUserClient();

        // Act
        var added = await UploadAsync(client, fileName, contentType);

        // Assert
        (await GetFileAsync(client, added.FileId)).Type.Should().Be(expectedType);
    }

    [Fact]
    public async Task Upload_without_a_file_returns_bad_request()
    {
        // Arrange
        using var form = new MultipartFormDataContent { { new StringContent("only a description"), "Description" } };

        // Act
        var response = await Client.PostAsync("/v1/media-files", form);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_with_too_long_description_returns_bad_request()
    {
        // Act
        var response = await PostFileAsync(Client, description: new string('d', 501));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        App.Storage.Keys.Should().BeEmpty();
    }

    [Fact]
    public async Task Upload_with_too_long_file_name_returns_bad_request()
    {
        // Act
        var response = await PostFileAsync(Client, fileName: new string('f', 101) + ".png");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        App.Storage.Keys.Should().BeEmpty();
    }

    [Fact]
    public async Task Upload_without_identity_returns_unauthorized()
    {
        // Act
        var response = await PostFileAsync(App.CreateClient());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- get ---------------------------------------------------------------------------------

    [Fact]
    public async Task Get_file_of_another_user_returns_media_file_not_owned()
    {
        // Arrange
        var added = await UploadAsync(CreateUserClient());
        var stranger = CreateUserClient();

        // Act
        var response = await stranger.GetAsync($"/v1/media-files/{added.FileId}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "media_file_not_owned");
    }

    [Fact]
    public async Task Get_unknown_file_returns_media_not_found()
    {
        // Act
        var response = await Client.GetAsync($"/v1/media-files/{Guid.NewGuid()}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "media_not_found");
    }

    [Fact]
    public async Task Get_file_without_identity_returns_unauthorized()
    {
        // Act
        var response = await App.CreateClient().GetAsync($"/v1/media-files/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- browse ------------------------------------------------------------------------------

    [Fact]
    public async Task Browse_files_returns_only_the_callers_files_newest_first()
    {
        // Arrange
        var client = CreateUserClient();
        var older = await UploadAsync(client, "older.png");
        await Task.Delay(20);
        var newer = await UploadAsync(client, "newer.png");
        await UploadAsync(CreateUserClient(), "someone-elses.png");

        // Act
        var page = await client.GetFromJsonAsync<PagedFilesResponse>("/v1/media-files?page=1&pageSize=100");

        // Assert
        page!.TotalCount.Should().Be(2);
        page.Items.Select(f => f.Id).Should().Equal(newer.FileId, older.FileId);
        page.Items.Should().OnlyContain(f => f.Url == $"/v1/media-files/{f.Id}/content");
    }

    [Fact]
    public async Task Browse_files_pages_the_results()
    {
        // Arrange
        var client = CreateUserClient();
        for (var index = 0; index < 3; index++)
        {
            await UploadAsync(client, $"file-{index}.png");
            await Task.Delay(20);
        }

        // Act
        var first = await client.GetFromJsonAsync<PagedFilesResponse>("/v1/media-files?page=1&pageSize=2");
        var second = await client.GetFromJsonAsync<PagedFilesResponse>("/v1/media-files?page=2&pageSize=2");

        // Assert
        first!.Items.Should().HaveCount(2);
        second!.Items.Should().ContainSingle();
        first.TotalCount.Should().Be(3);
        first.Items.Select(f => f.Id).Should().NotIntersectWith(second.Items.Select(f => f.Id));
    }

    [Fact]
    public async Task Browse_files_for_a_user_without_files_returns_an_empty_page()
    {
        // Act
        var page = await CreateUserClient().GetFromJsonAsync<PagedFilesResponse>("/v1/media-files");

        // Assert
        page!.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Browse_files_without_identity_returns_unauthorized()
    {
        // Act
        var response = await App.CreateClient().GetAsync("/v1/media-files");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- update ------------------------------------------------------------------------------

    [Fact]
    public async Task Update_file_changes_its_description()
    {
        // Arrange
        var client = CreateUserClient();
        var added = await UploadAsync(client, description: "Before");

        // Act
        var response = await client.PutAsJsonAsync("/v1/media-files", new { fileId = added.FileId, description = "After" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await GetFileAsync(client, added.FileId)).Description.Should().Be("After");
    }

    [Fact]
    public async Task Update_file_can_clear_the_description()
    {
        // Arrange
        var client = CreateUserClient();
        var added = await UploadAsync(client, description: "Before");

        // Act
        var response = await client.PutAsJsonAsync("/v1/media-files", new { fileId = added.FileId, description = (string?)null });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await GetFileAsync(client, added.FileId)).Description.Should().BeNull();
    }

    [Fact]
    public async Task Update_file_with_too_long_description_returns_bad_request()
    {
        // Arrange
        var client = CreateUserClient();
        var added = await UploadAsync(client);

        // Act
        var response = await client.PutAsJsonAsync("/v1/media-files", new
        {
            fileId = added.FileId,
            description = new string('d', 501),
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_unknown_file_returns_media_not_found()
    {
        // Act
        var response = await Client.PutAsJsonAsync("/v1/media-files", new { fileId = Guid.NewGuid(), description = "Text" });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "media_not_found");
    }

    // --- remove ------------------------------------------------------------------------------

    [Fact]
    public async Task Remove_file_deletes_the_metadata_and_the_stored_content()
    {
        // Arrange
        var client = CreateUserClient();
        var added = await UploadAsync(client);
        App.Storage.Contains(added.FilePath, BucketNames.UserUploads).Should().BeTrue();

        // Act
        var response = await DeleteWithBodyAsync(client, "/v1/media-files", new { fileId = added.FileId });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        App.Storage.Contains(added.FilePath, BucketNames.UserUploads).Should().BeFalse();
        var afterwards = await client.GetAsync($"/v1/media-files/{added.FileId}");
        await AssertErrorCodeAsync(afterwards, HttpStatusCode.BadRequest, "media_not_found");
    }

    [Fact]
    public async Task Remove_unknown_file_returns_media_not_found()
    {
        // Act
        var response = await DeleteWithBodyAsync(Client, "/v1/media-files", new { fileId = Guid.NewGuid() });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "media_not_found");
    }

    [Fact]
    public async Task Remove_file_with_empty_id_returns_bad_request()
    {
        // Act
        var response = await DeleteWithBodyAsync(Client, "/v1/media-files", new { fileId = Guid.Empty });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Remove_file_when_the_storage_fails_keeps_the_metadata()
    {
        // Arrange
        var client = CreateUserClient();
        var added = await UploadAsync(client);
        App.Storage.FailDeletes = true;

        // Act
        var response = await DeleteWithBodyAsync(client, "/v1/media-files", new { fileId = added.FileId });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "media_file_cannot_be_deleted");
        App.Storage.FailDeletes = false;
        (await GetFileAsync(client, added.FileId)).Id.Should().Be(added.FileId);
    }

    // --- download ----------------------------------------------------------------------------

    [Fact]
    public async Task Download_file_returns_the_stored_bytes_with_a_matching_content_type()
    {
        // Arrange
        var client = CreateUserClient();
        var content = new byte[] { 1, 1, 2, 3, 5, 8, 13 };
        var added = await UploadAsync(client, "photo.png", "image/png", content);

        // Act
        var response = await client.GetAsync($"/v1/media-files/{added.FileId}/content");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("image/png");
        (await response.Content.ReadAsByteArrayAsync()).Should().Equal(content);
    }

    [Fact]
    public async Task Download_file_with_an_unknown_extension_falls_back_to_octet_stream()
    {
        // Arrange
        var client = CreateUserClient();
        var added = await UploadAsync(client, "data.unknownext", "application/x-custom");

        // Act
        var response = await client.GetAsync($"/v1/media-files/{added.FileId}/content");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/octet-stream");
    }

    [Fact]
    public async Task Download_unknown_file_returns_media_not_found()
    {
        // Act
        var response = await Client.GetAsync($"/v1/media-files/{Guid.NewGuid()}/content");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "media_not_found");
    }

    [Fact]
    public async Task Download_file_without_identity_returns_unauthorized()
    {
        // Act
        var response = await App.CreateClient().GetAsync($"/v1/media-files/{Guid.NewGuid()}/content");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
