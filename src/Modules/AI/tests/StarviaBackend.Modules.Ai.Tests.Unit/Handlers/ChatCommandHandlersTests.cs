using FluentAssertions;
using StarviaBackend.Modules.Ai.Application.Chats.Commands.AddChat;
using StarviaBackend.Modules.Ai.Application.Chats.Commands.AddChatMessage;
using StarviaBackend.Modules.Ai.Application.Chats.Commands.RemoveChat;
using StarviaBackend.Modules.Ai.Application.Chats.Commands.UpdateChat;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Core.Chats.Exceptions;
using StarviaBackend.Modules.Ai.Tests.Unit.Fakes;

namespace StarviaBackend.Modules.Ai.Tests.Unit.Handlers;

public sealed class AddChatHandlerTests
{
    private readonly InMemoryChatRepository _chats = new();
    private readonly Guid _userId = Guid.NewGuid();

    private AddChatHandler CreateHandler() => new(_chats);

    [Theory]
    [InlineData(ChatType.AskAgent)]
    [InlineData(ChatType.PostContentGeneration)]
    public async Task HandleAsync_StoresAnActiveChatOwnedByTheCaller(ChatType type)
    {
        // Arrange
        var command = new AddChatCommand(new AddChatRequest(type, "  Ideas for December  "), _userId);

        // Act
        var result = await CreateHandler().HandleAsync(command);

        // Assert
        var chat = _chats.Chats.Should().ContainSingle().Subject;
        chat.Id.Should().Be(result.ChatId);
        chat.UserId.Should().Be(_userId);
        chat.Type.Should().Be(type);
        chat.Title.Should().Be("Ideas for December");
        chat.Status.Should().Be(ChatStatus.Active);
        chat.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_CalledTwice_CreatesTwoIndependentChats()
    {
        // Arrange
        var handler = CreateHandler();
        var request = new AddChatRequest(ChatType.AskAgent, "Same title");

        // Act
        var first = await handler.HandleAsync(new AddChatCommand(request, _userId));
        var second = await handler.HandleAsync(new AddChatCommand(request, _userId));

        // Assert
        first.ChatId.Should().NotBe(second.ChatId);
        _chats.Chats.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenTitleIsBlank_ThrowsAndStoresNothing()
    {
        // Act
        var act = () => CreateHandler().HandleAsync(new AddChatCommand(new AddChatRequest(ChatType.AskAgent, "  "), _userId));

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidChatTitleException>();
        exception.Which.Code.Should().Be("chat_title_invalid");
        _chats.Chats.Should().BeEmpty();
    }
}

public sealed class UpdateChatHandlerTests
{
    private readonly InMemoryChatRepository _chats = new();
    private readonly Guid _ownerId = Guid.NewGuid();

    private UpdateChatHandler CreateHandler(Guid? actingUserId = null) => new(_chats, TestAccess.For(actingUserId ?? _ownerId));

    private static UpdateChatCommand Command(Guid chatId, Guid userId, string title, ChatStatus status) =>
        new(chatId, new UpdateChatRequest(title, status), userId);

    [Fact]
    public async Task HandleAsync_RenamesTheChatAndPersistsIt()
    {
        // Arrange
        var chat = _chats.Seed(_ownerId, title: "Old title");

        // Act
        await CreateHandler().HandleAsync(Command(chat.Id, _ownerId, "  New title ", ChatStatus.Active));

        // Assert
        chat.Title.Should().Be("New title");
        chat.Status.Should().Be(ChatStatus.Active);
        _chats.UpdateCalls.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WhenStatusIsArchived_ArchivesTheChat()
    {
        // Arrange
        var chat = _chats.Seed(_ownerId);

        // Act
        await CreateHandler().HandleAsync(Command(chat.Id, _ownerId, chat.Title, ChatStatus.Archived));

        // Assert
        chat.Status.Should().Be(ChatStatus.Archived);
        _chats.UpdateCalls.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WhenStatusIsActive_RestoresAnArchivedChat()
    {
        // Arrange
        var chat = _chats.Seed(_ownerId);
        chat.Archive();

        // Act
        await CreateHandler().HandleAsync(Command(chat.Id, _ownerId, chat.Title, ChatStatus.Active));

        // Assert
        chat.Status.Should().Be(ChatStatus.Active);
    }

    [Fact]
    public async Task HandleAsync_WhenChatDoesNotExist_Throws()
    {
        // Act
        var act = () => CreateHandler().HandleAsync(Command(Guid.NewGuid(), _ownerId, "Title", ChatStatus.Active));

        // Assert
        var exception = await act.Should().ThrowAsync<ChatNotFoundException>();
        exception.Which.Code.Should().Be("chat_not_found");
        _chats.UpdateCalls.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WhenChatBelongsToAnotherUser_ThrowsAndChangesNothing()
    {
        // Arrange
        var chat = _chats.Seed(_ownerId, title: "Private");

        // Act
        var stranger = Guid.NewGuid();
        var act = () => CreateHandler(stranger).HandleAsync(Command(chat.Id, stranger, "Hijacked", ChatStatus.Archived));

        // Assert
        var exception = await act.Should().ThrowAsync<ChatAccessDeniedException>();
        exception.Which.Code.Should().Be("chat_access_denied");
        chat.Title.Should().Be("Private");
        chat.Status.Should().Be(ChatStatus.Active);
        _chats.UpdateCalls.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WhenTitleIsBlank_ThrowsWithoutPersisting()
    {
        // Arrange
        var chat = _chats.Seed(_ownerId, title: "Keep me");

        // Act
        var act = () => CreateHandler().HandleAsync(Command(chat.Id, _ownerId, " ", ChatStatus.Active));

        // Assert
        await act.Should().ThrowAsync<InvalidChatTitleException>();
        chat.Title.Should().Be("Keep me");
        _chats.UpdateCalls.Should().Be(0);
    }
}

public sealed class RemoveChatHandlerTests
{
    private readonly InMemoryChatRepository _chats = new();
    private readonly Guid _ownerId = Guid.NewGuid();

    private RemoveChatHandler CreateHandler(Guid? actingUserId = null) => new(_chats, TestAccess.For(actingUserId ?? _ownerId));

    [Fact]
    public async Task HandleAsync_RemovesOnlyTheRequestedChat()
    {
        // Arrange
        var toRemove = _chats.Seed(_ownerId, title: "Remove me");
        var toKeep = _chats.Seed(_ownerId, title: "Keep me");

        // Act
        await CreateHandler().HandleAsync(new RemoveChatCommand(toRemove.Id, _ownerId));

        // Assert
        _chats.Chats.Should().ContainSingle().Which.Should().BeSameAs(toKeep);
    }

    [Fact]
    public async Task HandleAsync_WhenChatDoesNotExist_Throws()
    {
        // Act
        var act = () => CreateHandler().HandleAsync(new RemoveChatCommand(Guid.NewGuid(), _ownerId));

        // Assert
        await act.Should().ThrowAsync<ChatNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenChatBelongsToAnotherUser_ThrowsAndKeepsTheChat()
    {
        // Arrange
        var chat = _chats.Seed(_ownerId);

        // Act
        var stranger = Guid.NewGuid();
        var act = () => CreateHandler(stranger).HandleAsync(new RemoveChatCommand(chat.Id, stranger));

        // Assert
        await act.Should().ThrowAsync<ChatAccessDeniedException>();
        _chats.Chats.Should().ContainSingle();
    }
}

public sealed class AddChatMessageHandlerTests
{
    private static readonly DateTime Now = new(2026, 10, 29, 12, 0, 0, DateTimeKind.Utc);

    private readonly InMemoryChatRepository _chats = new();
    private readonly FakeClock _clock = new(Now);
    private readonly Guid _ownerId = Guid.NewGuid();

    private AddChatMessageHandler CreateHandler(Guid? actingUserId = null) => new(_chats, TestAccess.For(actingUserId ?? _ownerId), _clock);

    private static AddChatMessageCommand Command(Guid chatId, Guid userId, string content) =>
        new(chatId, new AddChatMessageRequest(content), userId);

    [Fact]
    public async Task HandleAsync_StoresTheUserPromptVerbatimWithTheCurrentTime()
    {
        // Arrange
        var chat = _chats.Seed(_ownerId);

        // Act
        var result = await CreateHandler().HandleAsync(Command(chat.Id, _ownerId, "  Suggest a post about coffee  "));

        // Assert
        var message = chat.Messages.Should().ContainSingle().Subject;
        message.Id.Should().Be(result.MessageId);
        message.Sequence.Should().Be(result.Sequence).And.Be(1);
        message.Role.Should().Be(ChatMessageRole.User);
        message.Content.Should().Be("  Suggest a post about coffee  ");
        chat.LastMessageAt.Should().Be(Now);
        _chats.UpdateCalls.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_NextPrompts_ContinueTheSequence()
    {
        // Arrange
        var chat = _chats.Seed(_ownerId);
        var handler = CreateHandler();

        // Act
        await handler.HandleAsync(Command(chat.Id, _ownerId, "First"));
        _clock.UtcNow = Now.AddMinutes(5);
        var second = await handler.HandleAsync(Command(chat.Id, _ownerId, "Second"));

        // Assert
        second.Sequence.Should().Be(2);
        chat.Messages.Select(m => m.Content).Should().Equal("First", "Second");
        chat.LastMessageAt.Should().Be(Now.AddMinutes(5));
    }

    [Fact]
    public async Task HandleAsync_WhenChatDoesNotExist_Throws()
    {
        // Act
        var act = () => CreateHandler().HandleAsync(Command(Guid.NewGuid(), _ownerId, "Hello"));

        // Assert
        await act.Should().ThrowAsync<ChatNotFoundException>();
        _chats.UpdateCalls.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WhenChatBelongsToAnotherUser_ThrowsAndStoresNothing()
    {
        // Arrange
        var chat = _chats.Seed(_ownerId);

        // Act
        var stranger = Guid.NewGuid();
        var act = () => CreateHandler(stranger).HandleAsync(Command(chat.Id, stranger, "Hello"));

        // Assert
        await act.Should().ThrowAsync<ChatAccessDeniedException>();
        chat.Messages.Should().BeEmpty();
        _chats.UpdateCalls.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WhenChatIsArchived_ThrowsAndStoresNothing()
    {
        // Arrange
        var chat = _chats.Seed(_ownerId);
        chat.Archive();

        // Act
        var act = () => CreateHandler().HandleAsync(Command(chat.Id, _ownerId, "Hello"));

        // Assert
        var exception = await act.Should().ThrowAsync<ChatArchivedException>();
        exception.Which.Code.Should().Be("chat_archived");
        chat.Messages.Should().BeEmpty();
        _chats.UpdateCalls.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WhenContentIsBlank_ThrowsAndStoresNothing()
    {
        // Arrange
        var chat = _chats.Seed(_ownerId);

        // Act
        var act = () => CreateHandler().HandleAsync(Command(chat.Id, _ownerId, "   "));

        // Assert
        await act.Should().ThrowAsync<EmptyChatMessageException>();
        chat.Messages.Should().BeEmpty();
        _chats.UpdateCalls.Should().Be(0);
    }
}
