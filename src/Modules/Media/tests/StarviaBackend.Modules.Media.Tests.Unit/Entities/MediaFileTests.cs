using FluentAssertions;
using StarviaBackend.Modules.Media.Core.Media.Entities;
using StarviaBackend.Modules.Media.Core.Media.Enums;

namespace StarviaBackend.Modules.Media.Tests.Unit.Entities;

public sealed class MediaFileTests
{
    [Fact]
    public void Create_AssignsAllProvidedValues()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var file = MediaFile.Create(id, "photo.png", "user@example.com/photo_1.png", FileSource.User, FileType.Image, "A photo");

        // Assert
        file.Id.Should().Be(id);
        file.FileName.Should().Be("photo.png");
        file.FilePath.Should().Be("user@example.com/photo_1.png");
        file.Source.Should().Be(FileSource.User);
        file.Type.Should().Be(FileType.Image);
        file.Description.Should().Be("A photo");
    }

    [Fact]
    public void Create_AllowsMissingDescription()
    {
        // Act
        var file = MediaFile.Create(Guid.NewGuid(), "doc.pdf", "key", FileSource.System, FileType.Document, null);

        // Assert
        file.Description.Should().BeNull();
    }

    [Fact]
    public void UpdateDescription_ChangesOnlyTheDescription()
    {
        // Arrange
        var file = MediaFile.Create(Guid.NewGuid(), "photo.png", "key", FileSource.User, FileType.Image, "Old");

        // Act
        file.UpdateDescription("New");

        // Assert
        file.Description.Should().Be("New");
        file.FileName.Should().Be("photo.png");
        file.FilePath.Should().Be("key");
    }

    [Fact]
    public void UpdateDescription_AllowsClearingTheDescription()
    {
        // Arrange
        var file = MediaFile.Create(Guid.NewGuid(), "photo.png", "key", FileSource.User, FileType.Image, "Old");

        // Act
        file.UpdateDescription(null);

        // Assert
        file.Description.Should().BeNull();
    }

    [Fact]
    public void Update_ReplacesEveryMutableField()
    {
        // Arrange
        var id = Guid.NewGuid();
        var file = MediaFile.Create(id, "photo.png", "key", FileSource.User, FileType.Image, "Old");

        // Act
        file.Update("clip.mp4", "new-key", FileSource.Ai, FileType.Video, "Generated");

        // Assert
        file.Id.Should().Be(id);
        file.FileName.Should().Be("clip.mp4");
        file.FilePath.Should().Be("new-key");
        file.Source.Should().Be(FileSource.Ai);
        file.Type.Should().Be(FileType.Video);
        file.Description.Should().Be("Generated");
    }
}
