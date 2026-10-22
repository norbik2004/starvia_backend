using FluentAssertions;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;

namespace StarviaBackend.Modules.Posts.Tests.Unit.Entities;

public sealed class PostTests
{
    [Fact]
    public void Create_StartsAsDraftWithGivenTitle()
    {
        // Act
        var post = Post.Create("My first post");

        // Assert
        post.Id.Should().NotBeEmpty();
        post.Title.Should().Be("My first post");
        post.Body.Should().BeNull();
        post.Status.Should().Be(PostStatus.Draft);
        post.PostAttachments.Should().BeEmpty();
        post.PostPublications.Should().BeEmpty();
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        // Act
        var first = Post.Create("Post one");
        var second = Post.Create("Post two");

        // Assert
        first.Id.Should().NotBe(second.Id);
    }

    [Fact]
    public void Update_ReplacesTitleBodyAndStatus()
    {
        // Arrange
        var post = Post.Create("Old title");

        // Act
        post.Update("New title", "New body", PostStatus.Done);

        // Assert
        post.Title.Should().Be("New title");
        post.Body.Should().Be("New body");
        post.Status.Should().Be(PostStatus.Done);
    }

    [Fact]
    public void Update_AllowsClearingTheBody()
    {
        // Arrange
        var post = Post.Create("Title");
        post.Update("Title", "Some body", PostStatus.Draft);

        // Act
        post.Update("Title", null, PostStatus.Draft);

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
        var post = Post.Create("Title");

        // Act
        post.UpdateStatus(status);

        // Assert
        post.Status.Should().Be(status);
    }
}
