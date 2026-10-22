using FluentAssertions;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;

namespace StarviaBackend.Modules.Posts.Tests.Unit.Entities;

public sealed class PostAttachmentTests
{
    [Fact]
    public void MaxPerPost_IsFive()
    {
        PostAttachment.MaxPerPost.Should().Be(5);
    }

    [Fact]
    public void Create_AssignsPostFileAndOrder()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        // Act
        var attachment = PostAttachment.Create(postId, fileId, 3);

        // Assert
        attachment.Id.Should().NotBeEmpty();
        attachment.PostId.Should().Be(postId);
        attachment.FileId.Should().Be(fileId);
        attachment.Order.Should().Be(3);
    }

    [Fact]
    public void UpdateOrder_ChangesTheOrder()
    {
        // Arrange
        var attachment = PostAttachment.Create(Guid.NewGuid(), Guid.NewGuid(), 0);

        // Act
        attachment.UpdateOrder(4);

        // Assert
        attachment.Order.Should().Be(4);
    }
}

public sealed class PostPublicationTests
{
    [Fact]
    public void Create_AssignsAllProvidedValues()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userPlatformId = Guid.NewGuid();
        var publishedAt = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        // Act
        var publication = PostPublication.Create(
            postId, PostPublicationStatus.Published, userPlatformId, publishedAt, "external-1");

        // Assert
        publication.Id.Should().NotBeEmpty();
        publication.PostId.Should().Be(postId);
        publication.UserPlatformId.Should().Be(userPlatformId);
        publication.Status.Should().Be(PostPublicationStatus.Published);
        publication.PublishedAt.Should().Be(publishedAt);
        publication.ExternalPostId.Should().Be("external-1");
    }

    [Fact]
    public void Create_AllowsMissingPublicationDetails()
    {
        // Act
        var publication = PostPublication.Create(
            Guid.NewGuid(), PostPublicationStatus.Draft, Guid.NewGuid(), null, null);

        // Assert
        publication.PublishedAt.Should().BeNull();
        publication.ExternalPostId.Should().BeNull();
    }
}
