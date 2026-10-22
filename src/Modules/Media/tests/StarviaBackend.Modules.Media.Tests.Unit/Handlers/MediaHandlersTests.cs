using System.Text;
using FluentAssertions;
using StarviaBackend.Modules.Media.Application.MediaFiles.Commands.AddFile;
using StarviaBackend.Modules.Media.Application.MediaFiles.Commands.RemoveFile;
using StarviaBackend.Modules.Media.Application.MediaFiles.Commands.UpdateFile;
using StarviaBackend.Modules.Media.Core.Media.Enums;
using StarviaBackend.Modules.Media.Core.Media.Exceptions;
using StarviaBackend.Modules.Media.Tests.Unit.Fakes;
using StarviaBackend.Shared.Abstractions.Storage;

namespace StarviaBackend.Modules.Media.Tests.Unit.Handlers;

public sealed class AddFileHandlerTests
{
    private const string Email = "user@example.com";

    private readonly InMemoryMediaFileRepository _files = new();
    private readonly InMemoryFileStorageService _storage = new();

    private AddFileHandler CreateHandler() => new(_files, _storage);

    private static AddFileCommand Command(
        string fileName = "photo.png",
        string contentType = "image/png",
        byte[]? content = null,
        string? description = "A photo")
    {
        content ??= [1, 2, 3, 4];
        return new AddFileCommand(
            new AddFileRequest(fileName, contentType, content.Length, new MemoryStream(content), description),
            Guid.NewGuid(),
            Email);
    }

    [Fact]
    public async Task HandleAsync_UploadsTheContentAndStoresTheMetadata()
    {
        // Arrange
        var content = Encoding.UTF8.GetBytes("pretend this is a png");

        // Act
        var result = await CreateHandler().HandleAsync(Command(content: content));

        // Assert
        var upload = _storage.Uploads.Should().ContainSingle().Subject;
        upload.Bucket.Should().Be(BucketNames.UserUploads);
        upload.ContentType.Should().Be("image/png");
        _storage.Objects[(BucketNames.UserUploads, upload.Key)].Should().Equal(content);

        var file = _files.Files.Should().ContainSingle().Subject;
        file.Id.Should().Be(result.FileId);
        file.FileName.Should().Be("photo.png");
        file.FilePath.Should().Be(upload.Key);
        file.Description.Should().Be("A photo");
        file.Source.Should().Be(FileSource.User);
        result.FileName.Should().Be("photo.png");
        result.FilePath.Should().Be(upload.Key);
    }

    [Fact]
    public async Task HandleAsync_BuildsTheStorageKeyFromTheUsersEmailAndTheFileId()
    {
        // Act
        var result = await CreateHandler().HandleAsync(Command(fileName: "Holiday Photo.PNG"));

        // Assert
        result.FilePath.Should().Be($"{Email}/Holiday Photo_{result.FileId:N}.png");
    }

    [Fact]
    public async Task HandleAsync_KeepsOnlyTheFileNameOfAPath()
    {
        // Act
        var result = await CreateHandler().HandleAsync(Command(fileName: "../../etc/evil.png"));

        // Assert
        result.FileName.Should().Be("evil.png");
        result.FilePath.Should().StartWith($"{Email}/evil_");
    }

    [Fact]
    public async Task HandleAsync_UploadsEachFileUnderAUniqueKey()
    {
        // Act
        var first = await CreateHandler().HandleAsync(Command());
        var second = await CreateHandler().HandleAsync(Command());

        // Assert
        first.FilePath.Should().NotBe(second.FilePath);
        _storage.Objects.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("image/png", FileType.Image)]
    [InlineData("IMAGE/JPEG", FileType.Image)]
    [InlineData(" image/webp ", FileType.Image)]
    [InlineData("video/mp4", FileType.Video)]
    [InlineData("audio/mpeg", FileType.Audio)]
    [InlineData("text/plain", FileType.Document)]
    [InlineData("application/pdf", FileType.Document)]
    [InlineData("application/msword", FileType.Document)]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document", FileType.Document)]
    [InlineData("application/vnd.ms-excel", FileType.Document)]
    [InlineData("application/zip", FileType.Other)]
    [InlineData("application/octet-stream", FileType.Other)]
    public async Task HandleAsync_ResolvesTheFileTypeFromTheContentType(string contentType, FileType expected)
    {
        // Act
        await CreateHandler().HandleAsync(Command(contentType: contentType));

        // Assert
        _files.Files.Should().ContainSingle().Which.Type.Should().Be(expected);
    }

    [Fact]
    public async Task HandleAsync_WhenSavingMetadataFails_RemovesTheUploadedObjectAndRethrows()
    {
        // Arrange
        _files.AddFailure = new InvalidOperationException("database is down");

        // Act
        var act = () => CreateHandler().HandleAsync(Command());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("database is down");
        _storage.Objects.Should().BeEmpty();
        _storage.Deletes.Should().ContainSingle().Which.Bucket.Should().Be(BucketNames.UserUploads);
        _files.Files.Should().BeEmpty();
    }
}

public sealed class UpdateFileHandlerTests
{
    private readonly InMemoryMediaFileRepository _files = new();
    private readonly Guid _ownerId = Guid.NewGuid();

    private UpdateFileHandler CreateHandler() => new(_files);

    private static UpdateFileCommand Command(Guid fileId, Guid userId, string? description = "Updated") =>
        new(new UpdateFileRequest(fileId, description), userId);

    [Fact]
    public async Task HandleAsync_WhenFileDoesNotExist_Throws()
    {
        // Act
        var act = () => CreateHandler().HandleAsync(Command(Guid.NewGuid(), _ownerId));

        // Assert
        var exception = await act.Should().ThrowAsync<MediaNotFoundException>();
        exception.Which.Code.Should().Be("media_not_found");
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotTheCreator_ThrowsAndKeepsTheDescription()
    {
        // Arrange
        var file = _files.Seed(_ownerId, description: "Original");

        // Act
        var act = () => CreateHandler().HandleAsync(Command(file.Id, Guid.NewGuid()));

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        file.Description.Should().Be("Original");
        _files.Updated.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsTheCreator_UpdatesAndPersistsTheDescription()
    {
        // Arrange
        var file = _files.Seed(_ownerId, description: "Original");

        // Act
        var result = await CreateHandler().HandleAsync(Command(file.Id, _ownerId, "Updated"));

        // Assert
        result.FileId.Should().Be(file.Id);
        file.Description.Should().Be("Updated");
        _files.Updated.Should().ContainSingle().Which.Should().BeSameAs(file);
    }

    [Fact]
    public async Task HandleAsync_AllowsClearingTheDescription()
    {
        // Arrange
        var file = _files.Seed(_ownerId, description: "Original");

        // Act
        await CreateHandler().HandleAsync(Command(file.Id, _ownerId, null));

        // Assert
        file.Description.Should().BeNull();
    }
}

public sealed class RemoveFileHandlerTests
{
    private readonly InMemoryMediaFileRepository _files = new();
    private readonly InMemoryFileStorageService _storage = new();
    private readonly Guid _ownerId = Guid.NewGuid();

    private RemoveFileHandler CreateHandler() => new(_files, _storage);

    private static RemoveFileCommand Command(Guid fileId, Guid userId) => new(fileId, userId, "user@example.com");

    [Fact]
    public async Task HandleAsync_WhenFileDoesNotExist_Throws()
    {
        // Act
        var act = () => CreateHandler().HandleAsync(Command(Guid.NewGuid(), _ownerId));

        // Assert
        await act.Should().ThrowAsync<MediaNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_DeletesTheStoredObjectAndTheMetadata()
    {
        // Arrange
        var file = _files.Seed(_ownerId, filePath: "owner/file_1.png");
        _storage.Objects[(BucketNames.UserUploads, "owner/file_1.png")] = [1, 2, 3];

        // Act
        await CreateHandler().HandleAsync(Command(file.Id, _ownerId));

        // Assert
        _storage.Objects.Should().BeEmpty();
        _storage.Deletes.Should().ContainSingle().Which.Should().Be((BucketNames.UserUploads, "owner/file_1.png"));
        _files.Files.Should().BeEmpty();
        _files.Removed.Should().ContainSingle().Which.Should().BeSameAs(file);
    }

    [Fact]
    public async Task HandleAsync_WhenStorageDeletionFails_KeepsTheMetadataAndThrows()
    {
        // Arrange
        var file = _files.Seed(_ownerId);
        _storage.DeleteFailure = new IOException("storage is down");

        // Act
        var act = () => CreateHandler().HandleAsync(Command(file.Id, _ownerId));

        // Assert
        var exception = await act.Should().ThrowAsync<MediaFileCannotBeDeletedException>();
        exception.Which.Code.Should().Be("media_file_cannot_be_deleted");
        _files.Files.Should().ContainSingle();
        _files.Removed.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotTheCreator_ThrowsAndKeepsTheFile()
    {
        // Arrange
        var file = _files.Seed(_ownerId);

        // Act
        var act = () => CreateHandler().HandleAsync(Command(file.Id, Guid.NewGuid()));

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _files.Files.Should().ContainSingle();
        _storage.Deletes.Should().BeEmpty();
    }
}
