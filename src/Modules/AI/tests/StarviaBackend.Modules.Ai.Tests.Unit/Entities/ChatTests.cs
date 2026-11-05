using FluentAssertions;
using StarviaBackend.Modules.Ai.Core.Chats.Entities;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Core.Chats.Exceptions;

namespace StarviaBackend.Modules.Ai.Tests.Unit.Entities;

public sealed class ChatTests
{
    private static readonly DateTime Now = new(2026, 10, 29, 21, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(ChatType.AskAgent)]
    [InlineData(ChatType.PostContentGeneration)]
    public void Create_StartsActiveAndEmptyForTheGivenOwnerAndType(ChatType type)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var chat = Chat.Create(userId, type, "Launch campaign");

        // Assert
        chat.Id.Should().NotBeEmpty();
        chat.UserId.Should().Be(userId);
        chat.Type.Should().Be(type);
        chat.Title.Should().Be("Launch campaign");
        chat.Status.Should().Be(ChatStatus.Active);
        chat.PostId.Should().BeNull();
        chat.LastMessageAt.Should().BeNull();
        chat.Messages.Should().BeEmpty();
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "One").Id
            .Should().NotBe(Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "Two").Id);
    }

    [Fact]
    public void Create_TrimsTheTitle()
    {
        // Act
        var chat = Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "  Hello  ");

        // Assert
        chat.Title.Should().Be("Hello");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsBlankTitle(string title)
    {
        // Act
        var act = () => Chat.Create(Guid.NewGuid(), ChatType.AskAgent, title);

        // Assert
        act.Should().Throw<InvalidChatTitleException>().Which.Code.Should().Be("chat_title_invalid");
    }

    [Fact]
    public void Create_RejectsTitleLongerThanTheLimit()
    {
        // Arrange
        var title = new string('a', Chat.MaxTitleLength + 1);

        // Act
        var act = () => Chat.Create(Guid.NewGuid(), ChatType.AskAgent, title);

        // Assert
        act.Should().Throw<InvalidChatTitleException>();
    }

    [Fact]
    public void Create_AcceptsTitleOfExactlyTheLimit()
    {
        // Act
        var chat = Chat.Create(Guid.NewGuid(), ChatType.AskAgent, new string('a', Chat.MaxTitleLength));

        // Assert
        chat.Title.Should().HaveLength(Chat.MaxTitleLength);
    }

    [Fact]
    public void Create_RejectsEmptyUserId()
    {
        // Act
        var act = () => Chat.Create(Guid.Empty, ChatType.AskAgent, "Title");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_RejectsUnknownChatType()
    {
        // Act
        var act = () => Chat.Create(Guid.NewGuid(), (ChatType)0, "Title");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Rename_ReplacesAndTrimsTheTitle()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "Old");

        // Act
        chat.Rename("  New  ");

        // Assert
        chat.Title.Should().Be("New");
    }

    [Fact]
    public void Rename_RejectsBlankTitleAndKeepsTheOldOne()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "Old");

        // Act
        var act = () => chat.Rename(" ");

        // Assert
        act.Should().Throw<InvalidChatTitleException>();
        chat.Title.Should().Be("Old");
    }

    [Fact]
    public void ArchiveAndUnarchive_SwitchTheStatus()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "Title");

        // Act & Assert
        chat.Archive();
        chat.Status.Should().Be(ChatStatus.Archived);

        chat.Unarchive();
        chat.Status.Should().Be(ChatStatus.Active);
    }

    [Fact]
    public void LinkPost_SetsThePostOnAPostContentGenerationChat()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatType.PostContentGeneration, "Title");
        var postId = Guid.NewGuid();

        // Act
        chat.LinkPost(postId);

        // Assert
        chat.PostId.Should().Be(postId);
    }

    [Fact]
    public void LinkPost_IsRejectedForOtherChatTypes()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "Title");

        // Act
        var act = () => chat.LinkPost(Guid.NewGuid());

        // Assert
        act.Should().Throw<PostLinkNotSupportedException>().Which.Code.Should().Be("chat_post_link_not_supported");
        chat.PostId.Should().BeNull();
    }

    [Fact]
    public void LinkPost_RejectsEmptyPostId()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatType.PostContentGeneration, "Title");

        // Act
        var act = () => chat.LinkPost(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddUserMessage_StoresThePromptExactlyAsTyped()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "Title");

        // Act
        var message = chat.AddUserMessage("  How do I schedule a post?  ", Now);

        // Assert
        message.Id.Should().NotBeEmpty();
        message.ChatId.Should().Be(chat.Id);
        message.Role.Should().Be(ChatMessageRole.User);
        message.Content.Should().Be("  How do I schedule a post?  ");
        message.Sequence.Should().Be(1);
        message.Model.Should().BeNull();
        message.InputTokens.Should().BeNull();
        message.OutputTokens.Should().BeNull();
        chat.Messages.Should().ContainSingle().Which.Should().BeSameAs(message);
    }

    [Fact]
    public void AddAssistantMessage_StoresTheModelAndTokenUsage()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatType.PostContentGeneration, "Title");

        // Act
        var message = chat.AddAssistantMessage("Here is your post.", Now, "gpt-4o-mini", 120, 340);

        // Assert
        message.Role.Should().Be(ChatMessageRole.Assistant);
        message.Content.Should().Be("Here is your post.");
        message.Model.Should().Be("gpt-4o-mini");
        message.InputTokens.Should().Be(120);
        message.OutputTokens.Should().Be(340);
    }

    [Fact]
    public void AddSystemMessage_StoresTheInstructions()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "Title");

        // Act
        var message = chat.AddSystemMessage("You are a helpful assistant.", Now);

        // Assert
        message.Role.Should().Be(ChatMessageRole.System);
        message.Content.Should().Be("You are a helpful assistant.");
    }

    [Fact]
    public void AddMessage_AssignsIncreasingSequenceNumbersInOrder()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "Title");

        // Act
        chat.AddSystemMessage("Instructions", Now);
        chat.AddUserMessage("Question", Now);
        chat.AddAssistantMessage("Answer", Now);

        // Assert
        chat.Messages.Select(m => m.Sequence).Should().Equal(1, 2, 3);
        chat.Messages.Select(m => m.Role).Should().Equal(
            ChatMessageRole.System,
            ChatMessageRole.User,
            ChatMessageRole.Assistant);
    }

    [Fact]
    public void AddMessage_UpdatesLastMessageAt()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "Title");
        var later = Now.AddMinutes(5);

        // Act
        chat.AddUserMessage("Question", Now);
        chat.AddAssistantMessage("Answer", later);

        // Assert
        chat.LastMessageAt.Should().Be(later);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddMessage_RejectsBlankContentAndLeavesTheChatUntouched(string content)
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "Title");

        // Act
        var act = () => chat.AddUserMessage(content, Now);

        // Assert
        act.Should().Throw<EmptyChatMessageException>().Which.Code.Should().Be("chat_message_empty");
        chat.Messages.Should().BeEmpty();
        chat.LastMessageAt.Should().BeNull();
    }

    [Fact]
    public void AddMessage_IsRejectedWhenTheChatIsArchived()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "Title");
        chat.AddUserMessage("Question", Now);
        chat.Archive();

        // Act
        var act = () => chat.AddUserMessage("Another question", Now.AddMinutes(1));

        // Assert
        act.Should().Throw<ChatArchivedException>().Which.Code.Should().Be("chat_archived");
        chat.Messages.Should().ContainSingle();
        chat.LastMessageAt.Should().Be(Now);
    }

    [Fact]
    public void AddMessage_WorksAgainAfterUnarchiving()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "Title");
        chat.Archive();
        chat.Unarchive();

        // Act
        chat.AddUserMessage("Question", Now);

        // Assert
        chat.Messages.Should().ContainSingle();
    }

    [Fact]
    public void AddAssistantMessage_RejectsNegativeTokenCounts()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "Title");

        // Act & Assert
        var negativeInput = () => chat.AddAssistantMessage("Answer", Now, "model", -1, 10);
        var negativeOutput = () => chat.AddAssistantMessage("Answer", Now, "model", 10, -1);

        negativeInput.Should().Throw<ArgumentOutOfRangeException>();
        negativeOutput.Should().Throw<ArgumentOutOfRangeException>();
        chat.Messages.Should().BeEmpty();
    }

    [Fact]
    public void AddAssistantMessage_RejectsTooLongModelName()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatType.AskAgent, "Title");
        var model = new string('m', ChatMessage.MaxModelLength + 1);

        // Act
        var act = () => chat.AddAssistantMessage("Answer", Now, model);

        // Assert
        act.Should().Throw<ArgumentException>();
        chat.Messages.Should().BeEmpty();
    }
}
