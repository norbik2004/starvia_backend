using FluentAssertions;
using StarviaBackend.Modules.Posts.Application.Posts.Commands.AddPost;
using StarviaBackend.Modules.Posts.Application.Posts.Commands.RemovePost;
using StarviaBackend.Modules.Posts.Application.Posts.Commands.UpdatePost;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;
using StarviaBackend.Modules.Posts.Core.Posts.Exceptions;
using StarviaBackend.Modules.Posts.Tests.Unit.Fakes;

namespace StarviaBackend.Modules.Posts.Tests.Unit.Handlers;

public sealed class AddPostHandlerTests
{
    [Fact]
    public async Task HandleAsync_StoresADraftPostAndReturnsItsId()
    {
        // Arrange
        var repository = new InMemoryPostRepository();
        var handler = new AddPostHandler(repository);
        var command = new AddPostCommand(new AddPostRequest("Launch announcement"), Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        var stored = repository.Posts.Should().ContainSingle().Subject;
        result.PostId.Should().Be(stored.Id);
        stored.Title.Should().Be("Launch announcement");
        stored.Status.Should().Be(PostStatus.Draft);
    }
}

public sealed class UpdatePostHandlerTests
{
    private readonly InMemoryPostRepository _repository = new();
    private readonly Guid _ownerId = Guid.NewGuid();

    private UpdatePostHandler CreateHandler() => new(_repository);

    private static UpdatePostCommand Command(Guid postId, Guid userId) =>
        new(new UpdatePostRequest(postId, "Updated title", "Updated body", PostStatus.Done), userId);

    [Fact]
    public async Task HandleAsync_WhenPostDoesNotExist_Throws()
    {
        // Act
        var act = () => CreateHandler().HandleAsync(Command(Guid.NewGuid(), _ownerId));

        // Assert
        await act.Should().ThrowAsync<PostNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotTheCreator_ThrowsAndLeavesPostUntouched()
    {
        // Arrange
        var post = _repository.Seed(_ownerId, "Original title");

        // Act
        var act = () => CreateHandler().HandleAsync(Command(post.Id, Guid.NewGuid()));

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        post.Title.Should().Be("Original title");
        _repository.Updated.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsTheCreator_UpdatesAndPersistsThePost()
    {
        // Arrange
        var post = _repository.Seed(_ownerId, "Original title");

        // Act
        var result = await CreateHandler().HandleAsync(Command(post.Id, _ownerId));

        // Assert
        result.PostId.Should().Be(post.Id);
        post.Title.Should().Be("Updated title");
        post.Body.Should().Be("Updated body");
        post.Status.Should().Be(PostStatus.Done);
        _repository.Updated.Should().ContainSingle().Which.Should().BeSameAs(post);
    }
}

public sealed class RemovePostHandlerTests
{
    private readonly InMemoryPostRepository _repository = new();
    private readonly Guid _ownerId = Guid.NewGuid();

    private RemovePostHandler CreateHandler() => new(_repository);

    [Fact]
    public async Task HandleAsync_WhenPostDoesNotExist_Throws()
    {
        // Act
        var act = () => CreateHandler().HandleAsync(new RemovePostCommand(new RemovePostRequest(Guid.NewGuid()), _ownerId));

        // Assert
        await act.Should().ThrowAsync<PostNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotTheCreator_ThrowsAndKeepsThePost()
    {
        // Arrange
        var post = _repository.Seed(_ownerId);

        // Act
        var act = () => CreateHandler().HandleAsync(new RemovePostCommand(new RemovePostRequest(post.Id), Guid.NewGuid()));

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _repository.Posts.Should().ContainSingle();
        _repository.Removed.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsTheCreator_RemovesThePost()
    {
        // Arrange
        var post = _repository.Seed(_ownerId);

        // Act
        await CreateHandler().HandleAsync(new RemovePostCommand(new RemovePostRequest(post.Id), _ownerId));

        // Assert
        _repository.Posts.Should().BeEmpty();
        _repository.Removed.Should().ContainSingle().Which.Should().BeSameAs(post);
    }
}
