using FluentAssertions;
using StarviaBackend.Modules.Media.Integrations;
using StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.AddPostAttachment;
using StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.AddPostAttachments;
using StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.RemovePostAttachment;
using StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.UpdatePostAttachmentOrder;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Core.Posts.Exceptions;
using StarviaBackend.Modules.Posts.Tests.Unit.Fakes;

namespace StarviaBackend.Modules.Posts.Tests.Unit.Handlers;

public sealed class AddPostAttachmentsHandlerTests
{
    private readonly InMemoryPostRepository _posts = new();
    private readonly InMemoryPostAttachmentRepository _attachments = new();
    private readonly FakeMediaClient _media = new();
    private readonly Guid _ownerId = Guid.NewGuid();

    private AddPostAttachmentsHandler CreateHandler() => new(_posts, _attachments, _media);

    private static AddPostAttachmentsCommand Command(Guid postId, Guid userId, params Guid[] fileIds) =>
        new(new AddPostAttachmentsRequest(postId, fileIds.ToList()), userId);

    [Fact]
    public async Task HandleAsync_WhenPostDoesNotExist_Throws()
    {
        // Act
        var act = () => CreateHandler().HandleAsync(Command(Guid.NewGuid(), _ownerId, Guid.NewGuid()));

        // Assert
        await act.Should().ThrowAsync<PostNotFoundException>();
        _media.Calls.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotTheCreator_ThrowsWithoutCallingMedia()
    {
        // Arrange
        var post = _posts.Seed(_ownerId);

        // Act
        var act = () => CreateHandler().HandleAsync(Command(post.Id, Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _media.Calls.Should().BeEmpty();
        _attachments.Attachments.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenPostWouldExceedTheLimit_Throws()
    {
        // Arrange
        var post = _posts.Seed(_ownerId);
        for (var order = 0; order < 4; order++)
        {
            _attachments.Seed(post.Id, order);
        }

        // Act
        var act = () => CreateHandler().HandleAsync(Command(post.Id, _ownerId, Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        await act.Should().ThrowAsync<TooManyPostAttachmentsException>();
        _attachments.Attachments.Should().HaveCount(4);
    }

    [Fact]
    public async Task HandleAsync_AppendsAttachmentsAfterTheExistingOnes()
    {
        // Arrange
        var post = _posts.Seed(_ownerId);
        _attachments.Seed(post.Id, 0);
        _attachments.Seed(post.Id, 1);
        var newFiles = new[] { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var result = await CreateHandler().HandleAsync(Command(post.Id, _ownerId, newFiles));

        // Assert
        result.postAttachmentIds.Should().HaveCount(2);
        var created = _attachments.Attachments.Where(a => result.postAttachmentIds.Contains(a.Id)).OrderBy(a => a.Order).ToList();
        created.Select(a => a.FileId).Should().Equal(newFiles);
        created.Select(a => a.Order).Should().Equal(2, 3);
        created.Should().OnlyContain(a => a.PostId == post.Id);
    }

    [Fact]
    public async Task HandleAsync_StartsAtZeroWhenThePostHasNoAttachments()
    {
        // Arrange
        var post = _posts.Seed(_ownerId);

        // Act
        await CreateHandler().HandleAsync(Command(post.Id, _ownerId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        _attachments.Attachments.Select(a => a.Order).Should().Equal(0, 1, 2);
    }

    [Fact]
    public async Task HandleAsync_AsksMediaForTheCallersFiles()
    {
        // Arrange
        var post = _posts.Seed(_ownerId);
        var fileId = Guid.NewGuid();

        // Act
        await CreateHandler().HandleAsync(Command(post.Id, _ownerId, fileId));

        // Assert
        var call = _media.Calls.Should().ContainSingle().Subject;
        call.UserId.Should().Be(_ownerId);
        call.FileIds.Should().Equal(fileId);
    }

    [Fact]
    public async Task HandleAsync_WhenMediaDoesNotReturnAFile_ThrowsAndStoresNothing()
    {
        // Arrange
        var post = _posts.Seed(_ownerId);
        var existingFile = Guid.NewGuid();
        var missingFile = Guid.NewGuid();
        _media.MissingFileIds.Add(missingFile);

        // Act
        var act = () => CreateHandler().HandleAsync(Command(post.Id, _ownerId, existingFile, missingFile));

        // Assert
        var exception = await act.Should().ThrowAsync<MediaIntegrationException>();
        exception.Which.Message.Should().Contain(missingFile.ToString());
        _attachments.Attachments.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenMediaRejectsTheUser_PropagatesTheFailure()
    {
        // Arrange
        var post = _posts.Seed(_ownerId);
        _media.Failure = new MediaIntegrationNotOwnerException("User is not the owner of file.");

        // Act
        var act = () => CreateHandler().HandleAsync(Command(post.Id, _ownerId, Guid.NewGuid()));

        // Assert
        await act.Should().ThrowAsync<MediaIntegrationNotOwnerException>();
        _attachments.Attachments.Should().BeEmpty();
    }
}

public sealed class RemovePostAttachmentHandlerTests
{
    private readonly InMemoryPostRepository _posts = new();
    private readonly InMemoryPostAttachmentRepository _attachments = new();
    private readonly Guid _ownerId = Guid.NewGuid();

    private RemovePostAttachmentHandler CreateHandler() => new(_posts, _attachments);

    private static RemovePostAttachmentCommand Command(Guid postId, Guid attachmentId, Guid userId) =>
        new(new RemovePostAttachmentRequest(postId, attachmentId), userId);

    [Fact]
    public async Task HandleAsync_WhenPostDoesNotExist_Throws()
    {
        // Act
        var act = () => CreateHandler().HandleAsync(Command(Guid.NewGuid(), Guid.NewGuid(), _ownerId));

        // Assert
        await act.Should().ThrowAsync<PostNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotTheCreator_Throws()
    {
        // Arrange
        var post = _posts.Seed(_ownerId);
        var attachment = _attachments.Seed(post.Id, 0);

        // Act
        var act = () => CreateHandler().HandleAsync(Command(post.Id, attachment.Id, Guid.NewGuid()));

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _attachments.Attachments.Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WhenAttachmentBelongsToAnotherPost_Throws()
    {
        // Arrange
        var post = _posts.Seed(_ownerId);
        var otherPost = _posts.Seed(_ownerId, "Other post");
        var foreignAttachment = _attachments.Seed(otherPost.Id, 0);

        // Act
        var act = () => CreateHandler().HandleAsync(Command(post.Id, foreignAttachment.Id, _ownerId));

        // Assert
        await act.Should().ThrowAsync<PostAttachmentNotFoundException>();
        _attachments.Attachments.Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_RemovesTheAttachmentAndClosesTheGapInTheOrder()
    {
        // Arrange
        var post = _posts.Seed(_ownerId);
        var first = _attachments.Seed(post.Id, 0);
        var second = _attachments.Seed(post.Id, 1);
        var third = _attachments.Seed(post.Id, 2);

        // Act
        await CreateHandler().HandleAsync(Command(post.Id, second.Id, _ownerId));

        // Assert
        _attachments.Removed.Should().ContainSingle().Which.Should().BeSameAs(second);
        first.Order.Should().Be(0);
        third.Order.Should().Be(1);
    }
}

public sealed class UpdatePostAttachmentOrderHandlerTests
{
    private readonly InMemoryPostRepository _posts = new();
    private readonly InMemoryPostAttachmentRepository _attachments = new();
    private readonly Guid _ownerId = Guid.NewGuid();

    private UpdatePostAttachmentOrderHandler CreateHandler() => new(_posts, _attachments);

    private static UpdatePostAttachmentOrderCommand Command(Guid postId, Guid attachmentId, int order, Guid userId) =>
        new(new UpdatePostAttachmentOrderRequest(postId, attachmentId, order), userId);

    private (Post Post, PostAttachment A, PostAttachment B, PostAttachment C) SeedPostWithThreeAttachments()
    {
        var post = _posts.Seed(_ownerId);
        return (post, _attachments.Seed(post.Id, 0), _attachments.Seed(post.Id, 1), _attachments.Seed(post.Id, 2));
    }

    [Fact]
    public async Task HandleAsync_WhenPostDoesNotExist_Throws()
    {
        // Act
        var act = () => CreateHandler().HandleAsync(Command(Guid.NewGuid(), Guid.NewGuid(), 0, _ownerId));

        // Assert
        await act.Should().ThrowAsync<PostNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotTheCreator_Throws()
    {
        // Arrange
        var (post, a, _, _) = SeedPostWithThreeAttachments();

        // Act
        var act = () => CreateHandler().HandleAsync(Command(post.Id, a.Id, 1, Guid.NewGuid()));

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        a.Order.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WhenAttachmentDoesNotExist_Throws()
    {
        // Arrange
        var (post, _, _, _) = SeedPostWithThreeAttachments();

        // Act
        var act = () => CreateHandler().HandleAsync(Command(post.Id, Guid.NewGuid(), 1, _ownerId));

        // Assert
        await act.Should().ThrowAsync<PostAttachmentNotFoundException>();
    }

    [Theory]
    [InlineData(3)]
    [InlineData(10)]
    public async Task HandleAsync_WhenOrderIsOutOfRange_ThrowsAndKeepsTheOrder(int order)
    {
        // Arrange
        var (post, a, b, c) = SeedPostWithThreeAttachments();

        // Act
        var act = () => CreateHandler().HandleAsync(Command(post.Id, a.Id, order, _ownerId));

        // Assert
        await act.Should().ThrowAsync<InvalidPostAttachmentOrderException>();
        new[] { a.Order, b.Order, c.Order }.Should().Equal(0, 1, 2);
        _attachments.UpdateRangeCalls.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_MovingTheLastAttachmentToTheFront_ShiftsTheOthersBack()
    {
        // Arrange
        var (post, a, b, c) = SeedPostWithThreeAttachments();

        // Act
        await CreateHandler().HandleAsync(Command(post.Id, c.Id, 0, _ownerId));

        // Assert
        c.Order.Should().Be(0);
        a.Order.Should().Be(1);
        b.Order.Should().Be(2);
        _attachments.UpdateRangeCalls.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_MovingTheFirstAttachmentToTheEnd_ShiftsTheOthersForward()
    {
        // Arrange
        var (post, a, b, c) = SeedPostWithThreeAttachments();

        // Act
        await CreateHandler().HandleAsync(Command(post.Id, a.Id, 2, _ownerId));

        // Assert
        b.Order.Should().Be(0);
        c.Order.Should().Be(1);
        a.Order.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_KeepingTheSamePosition_DoesNotChangeTheOrder()
    {
        // Arrange
        var (post, a, b, c) = SeedPostWithThreeAttachments();

        // Act
        await CreateHandler().HandleAsync(Command(post.Id, b.Id, 1, _ownerId));

        // Assert
        new[] { a.Order, b.Order, c.Order }.Should().Equal(0, 1, 2);
    }
}
