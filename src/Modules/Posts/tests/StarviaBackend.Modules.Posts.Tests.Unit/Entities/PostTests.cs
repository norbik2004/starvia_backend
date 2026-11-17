using FluentAssertions;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;
using StarviaBackend.Modules.Posts.Core.Posts.Exceptions;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Posts.Tests.Unit.Entities;

public sealed class PostTests
{
    [Fact]
    public void Create_StartsAsDraftWithGivenTitleAndPlatforms()
    {
        // Act
        var post = Post.Create("My first post", [PlatformType.Facebook, PlatformType.Instagram]);

        // Assert
        post.Id.Should().NotBeEmpty();
        post.Title.Should().Be("My first post");
        post.Body.Should().BeNull();
        post.Status.Should().Be(PostStatus.Draft);
        post.Platforms.Should().Equal(PlatformType.Facebook, PlatformType.Instagram);
        post.PostAttachments.Should().BeEmpty();
        post.PostPublications.Should().BeEmpty();
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        // Act
        var first = Post.Create("Post one", [PlatformType.LinkedIn]);
        var second = Post.Create("Post two", [PlatformType.LinkedIn]);

        // Assert
        first.Id.Should().NotBe(second.Id);
    }

    [Fact]
    public void Create_WithDuplicatePlatforms_Throws()
    {
        // Act
        var act = () => Post.Create("Title", [PlatformType.LinkedIn, PlatformType.LinkedIn]);

        // Assert
        act.Should().Throw<DuplicatePlatformTypeException>();
    }

    [Fact]
    public void Update_ReplacesTitleBodyStatusAndPlatforms()
    {
        // Arrange
        var post = Post.Create("Old title", [PlatformType.LinkedIn]);

        // Act
        post.Update("New title", "New body", PostStatus.Done, [PlatformType.Instagram, PlatformType.Facebook]);

        // Assert
        post.Title.Should().Be("New title");
        post.Body.Should().Be("New body");
        post.Status.Should().Be(PostStatus.Done);
        post.Platforms.Should().Equal(PlatformType.Instagram, PlatformType.Facebook);
    }

    [Fact]
    public void Update_AllowsClearingTheBody()
    {
        // Arrange
        var post = Post.Create("Title", [PlatformType.LinkedIn]);
        post.Update("Title", "Some body", PostStatus.Draft, [PlatformType.LinkedIn]);

        // Act
        post.Update("Title", null, PostStatus.Draft, [PlatformType.LinkedIn]);

        // Assert
        post.Body.Should().BeNull();
    }

    [Theory]
    [InlineData(PostStatus.Generated)]
    [InlineData(PostStatus.Draft)]
    [InlineData(PostStatus.Done)]
    public void UpdateStatus_SetsTheStatus(PostStatus status)
    {
        // Arrange
        var post = Post.Create("Title", [PlatformType.LinkedIn]);

        // Act
        post.UpdateStatus(status);

        // Assert
        post.Status.Should().Be(status);
    }
}
