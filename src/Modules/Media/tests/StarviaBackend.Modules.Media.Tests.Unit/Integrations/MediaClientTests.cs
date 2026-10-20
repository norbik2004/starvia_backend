using System.Net;
using FluentAssertions;
using StarviaBackend.Modules.Media.Integrations;
using StarviaBackend.Modules.Media.Tests.Unit.Fakes;

namespace StarviaBackend.Modules.Media.Tests.Unit.Integrations;

public sealed class MediaClientTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _fileId = Guid.NewGuid();

    private static MediaClient CreateClient(StubHttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("http://media.test/") });

    private object FileBody(string? createdBy = null) => new
    {
        id = _fileId,
        fileName = "photo.png",
        url = $"/v1/media-files/{_fileId}/content",
        createdAt = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc),
        createdBy = createdBy ?? _userId.ToString(),
    };

    private static object ErrorBody(string code) => new { errors = new[] { new { code, message = "boom" } } };

    [Fact]
    public async Task GetFilesAsync_ReturnsFilesOwnedByTheUser()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, FileBody());
        var client = CreateClient(handler);

        // Act
        var files = await client.GetFilesAsync(_userId, [_fileId]);

        // Assert
        var file = files.Should().ContainSingle().Subject;
        file.Id.Should().Be(_fileId);
        file.FileName.Should().Be("photo.png");
        file.Url.Should().Be($"/v1/media-files/{_fileId}/content");
        file.CreatedAt.Should().Be(new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc));
        handler.RequestedUris.Should().ContainSingle().Which.AbsolutePath.Should().Be($"/v1/media-files/{_fileId}");
    }

    [Fact]
    public async Task GetFilesAsync_RequestsEachDistinctFileOnce()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, FileBody());
        var client = CreateClient(handler);

        // Act
        var files = await client.GetFilesAsync(_userId, [_fileId, _fileId]);

        // Assert
        files.Should().ContainSingle();
        handler.RequestedUris.Should().ContainSingle();
    }

    [Fact]
    public async Task GetFilesAsync_WithNoFiles_DoesNotCallTheApi()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, FileBody());
        var client = CreateClient(handler);

        // Act
        var files = await client.GetFilesAsync(_userId, []);

        // Assert
        files.Should().BeEmpty();
        handler.RequestedUris.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilesAsync_WhenPayloadCreatorIsAnotherUser_ThrowsNotOwner()
    {
        // Arrange
        var client = CreateClient(StubHttpMessageHandler.Json(HttpStatusCode.OK, FileBody(createdBy: Guid.NewGuid().ToString())));

        // Act
        var act = () => client.GetFilesAsync(_userId, [_fileId]);

        // Assert
        var exception = await act.Should().ThrowAsync<MediaIntegrationNotOwnerException>();
        exception.Which.Code.Should().Be("media_file_not_owned");
    }

    [Fact]
    public async Task GetFilesAsync_WhenPayloadHasNoCreator_ThrowsNotOwner()
    {
        // Arrange
        var body = new { id = _fileId, fileName = "photo.png", url = "/x", createdAt = DateTime.UtcNow, createdBy = (string?)null };
        var client = CreateClient(StubHttpMessageHandler.Json(HttpStatusCode.OK, body));

        // Act
        var act = () => client.GetFilesAsync(_userId, [_fileId]);

        // Assert
        await act.Should().ThrowAsync<MediaIntegrationNotOwnerException>();
    }

    [Fact]
    public async Task GetFilesAsync_On404_ThrowsNotFound()
    {
        // Arrange
        var client = CreateClient(StubHttpMessageHandler.Text(HttpStatusCode.NotFound, string.Empty));

        // Act
        var act = () => client.GetFilesAsync(_userId, [_fileId]);

        // Assert
        var exception = await act.Should().ThrowAsync<MediaIntegrationException>();
        exception.Which.Code.Should().Be("media_integration_failed");
        exception.Which.Message.Should().Contain(_fileId.ToString()).And.Contain("not found");
    }

    [Fact]
    public async Task GetFilesAsync_OnMediaNotFoundErrorCode_ThrowsNotFound()
    {
        // Arrange
        var client = CreateClient(StubHttpMessageHandler.Json(HttpStatusCode.BadRequest, ErrorBody("media_not_found")));

        // Act
        var act = () => client.GetFilesAsync(_userId, [_fileId]);

        // Assert
        var exception = await act.Should().ThrowAsync<MediaIntegrationException>();
        exception.Which.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task GetFilesAsync_OnMediaFileNotOwnedErrorCode_ThrowsNotOwner()
    {
        // Arrange
        var client = CreateClient(StubHttpMessageHandler.Json(HttpStatusCode.BadRequest, ErrorBody("media_file_not_owned")));

        // Act
        var act = () => client.GetFilesAsync(_userId, [_fileId]);

        // Assert
        await act.Should().ThrowAsync<MediaIntegrationNotOwnerException>();
    }

    [Fact]
    public async Task GetFilesAsync_OnUnknownServerError_ThrowsGenericFailure()
    {
        // Arrange
        var client = CreateClient(StubHttpMessageHandler.Text(HttpStatusCode.InternalServerError, "<html>oops</html>"));

        // Act
        var act = () => client.GetFilesAsync(_userId, [_fileId]);

        // Assert
        var exception = await act.Should().ThrowAsync<MediaIntegrationException>();
        exception.Which.Message.Should().Be("Media API returned 500.");
    }

    [Fact]
    public async Task GetFilesAsync_OnUnknownErrorCode_ThrowsGenericFailure()
    {
        // Arrange
        var client = CreateClient(StubHttpMessageHandler.Json(HttpStatusCode.BadRequest, ErrorBody("something_else")));

        // Act
        var act = () => client.GetFilesAsync(_userId, [_fileId]);

        // Assert
        var exception = await act.Should().ThrowAsync<MediaIntegrationException>();
        exception.Which.Message.Should().Be("Media API returned 400.");
    }

    [Fact]
    public async Task GetFilesAsync_WhenApiIsUnreachable_ThrowsUnreachable()
    {
        // Arrange
        var client = CreateClient(StubHttpMessageHandler.Throwing(new HttpRequestException("connection refused")));

        // Act
        var act = () => client.GetFilesAsync(_userId, [_fileId]);

        // Assert
        var exception = await act.Should().ThrowAsync<MediaIntegrationException>();
        exception.Which.Message.Should().Contain("unreachable").And.Contain("connection refused");
    }

    [Fact]
    public async Task GetFilesAsync_WhenRequestTimesOut_ThrowsUnreachable()
    {
        // Arrange
        var client = CreateClient(StubHttpMessageHandler.Throwing(new TaskCanceledException("timeout")));

        // Act
        var act = () => client.GetFilesAsync(_userId, [_fileId]);

        // Assert
        var exception = await act.Should().ThrowAsync<MediaIntegrationException>();
        exception.Which.Message.Should().Contain("unreachable");
    }

    [Fact]
    public async Task GetFilesAsync_WithEmptyUserId_Throws()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, FileBody());
        var client = CreateClient(handler);

        // Act
        var act = () => client.GetFilesAsync(Guid.Empty, [_fileId]);

        // Assert
        await act.Should().ThrowAsync<MediaIntegrationException>().WithMessage("User id cannot be empty.");
        handler.RequestedUris.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilesAsync_WithEmptyFileId_Throws()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, FileBody());
        var client = CreateClient(handler);

        // Act
        var act = () => client.GetFilesAsync(_userId, [Guid.Empty]);

        // Assert
        await act.Should().ThrowAsync<MediaIntegrationException>().WithMessage("File id cannot be empty.");
        handler.RequestedUris.Should().BeEmpty();
    }
}
