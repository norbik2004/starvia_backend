using FluentAssertions;
using Microsoft.Extensions.AI;
using StarviaBackend.Modules.Ai.Application.PostGeneration;
using StarviaBackend.Modules.Ai.Application.PostGeneration.Commands.GeneratePost;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Core.Chats.Exceptions;
using StarviaBackend.Modules.Ai.Infrastructure.GoogleAiStudio;
using StarviaBackend.Modules.Ai.Tests.Unit.Fakes;

namespace StarviaBackend.Modules.Ai.Tests.Unit.Handlers;

public sealed class GeneratePostHandlerTests
{
    private static readonly DateTime Now = new(2026, 10, 30, 12, 0, 0, DateTimeKind.Utc);

    private readonly InMemoryChatRepository _chats = new();
    private readonly FakeChatClient _model = new();
    private readonly FakeClock _clock = new(Now);
    private readonly Guid _userId = Guid.NewGuid();

    private GeneratePostHandler CreateHandler() => new(_chats, _model, _clock);

    private GeneratePostCommand Command(string prompt = "A post about coffee", Guid? chatId = null, Guid? userId = null) =>
        new(new GeneratePostRequest(prompt, chatId), userId ?? _userId);

    // --- a new chat --------------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_WithoutAChat_ReturnsThePostAndTheDetailsOfTheAnswer()
    {
        // Act
        var result = await CreateHandler().HandleAsync(Command());

        // Assert
        var chat = _chats.Chats.Should().ContainSingle().Subject;
        result.ChatId.Should().Be(chat.Id);
        result.MessageId.Should().Be(chat.Messages.Last().Id);
        result.Title.Should().Be(FakeChatClient.DefaultTitle);
        result.Body.Should().Be(FakeChatClient.DefaultBody);
        result.Model.Should().Be("fake/model-1");
        result.InputTokens.Should().Be(42);
        result.OutputTokens.Should().Be(17);
    }

    [Fact]
    public async Task HandleAsync_WithoutAChat_StoresTheWholeExchangeInANewPostGenerationChat()
    {
        // Act
        await CreateHandler().HandleAsync(Command("  A post about coffee  "));

        // Assert
        var chat = _chats.Chats.Should().ContainSingle().Subject;
        chat.UserId.Should().Be(_userId);
        chat.Type.Should().Be(ChatType.PostContentGeneration);
        chat.Status.Should().Be(ChatStatus.Active);
        chat.Title.Should().Be("A post about coffee");
        chat.LastMessageAt.Should().Be(Now);

        var messages = chat.Messages.OrderBy(m => m.Sequence).ToList();
        messages.Select(m => (m.Sequence, m.Role)).Should().Equal(
            (1, ChatMessageRole.System),
            (2, ChatMessageRole.User),
            (3, ChatMessageRole.Assistant));
        messages[0].Content.Should().Be(PostGenerationPrompt.Instructions);
        messages[1].Content.Should().Be("  A post about coffee  ");
        messages[2].Content.Should().Be(_model.Reply);
        messages[2].Model.Should().Be("fake/model-1");
        messages[2].InputTokens.Should().Be(42);
        messages[2].OutputTokens.Should().Be(17);
        messages[0].Model.Should().BeNull();
        messages[1].Model.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_AsksTheModelForJsonWithTheInstructionsAndThePrompt()
    {
        // Act
        await CreateHandler().HandleAsync(Command("A post about coffee"));

        // Assert
        var request = _model.Requests.Should().ContainSingle().Subject;
        request.Options.Should().NotBeNull();
        request.Options!.ResponseFormat.Should().Be(ChatResponseFormat.Json);
        request.Options.Temperature.Should().BeNull("Gemini 3 is meant to run at its default and newer models ignore it");
        request.Options.TopP.Should().BeNull();
        request.Options.TopK.Should().BeNull();
        request.Options.MaxOutputTokens.Should().Be(PostGenerationPrompt.MaxTokens);
        request.Options.ModelId.Should().BeNull("the model configured for the client is used");
        request.Messages.Select(m => (m.Role, m.Text)).Should().Equal(
            (ChatRole.System, PostGenerationPrompt.Instructions),
            (ChatRole.User, "A post about coffee"));
    }

    [Fact]
    public void Instructions_DescribeTheJsonAnswerTheParserExpectsAndTheLimitsOfAPost()
    {
        // Assert
        PostGenerationPrompt.Instructions.Should()
            .Contain("JSON").And.Contain("\"title\"").And.Contain("\"body\"")
            .And.Contain(PostGenerationPrompt.MaxTitleLength.ToString())
            .And.Contain(PostGenerationPrompt.MaxBodyLength.ToString());
    }

    [Fact]
    public async Task HandleAsync_WithoutAChat_DoesNotUpdateAnything()
    {
        // Act
        await CreateHandler().HandleAsync(Command());

        // Assert
        _chats.UpdateCalls.Should().Be(0, "a new chat is added, not updated");
    }

    [Fact]
    public async Task HandleAsync_CalledTwiceWithoutAChat_CreatesTwoIndependentChats()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        var first = await handler.HandleAsync(Command("Same prompt"));
        var second = await handler.HandleAsync(Command("Same prompt"));

        // Assert
        first.ChatId.Should().NotBe(second.ChatId);
        _chats.Chats.Should().HaveCount(2);
    }

    // --- chat title --------------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_NamesTheChatAfterTheOneLinePromptWithoutExtraWhitespace()
    {
        // Act
        await CreateHandler().HandleAsync(Command("A post\r\nabout   coffee\n\n for  Monday"));

        // Assert
        _chats.Chats.Single().Title.Should().Be("A post about coffee for Monday");
    }

    [Fact]
    public async Task HandleAsync_ShortensALongPromptToTheMaximumTitleLength()
    {
        // Act
        await CreateHandler().HandleAsync(Command(new string('a', 400)));

        // Assert
        _chats.Chats.Single().Title.Should().HaveLength(150).And.EndWith("…");
    }

    [Fact]
    public async Task HandleAsync_KeepsAPromptThatFitsTheTitleUntouched()
    {
        // Arrange
        var prompt = new string('a', 150);

        // Act
        await CreateHandler().HandleAsync(Command(prompt));

        // Assert
        _chats.Chats.Single().Title.Should().Be(prompt);
    }

    [Fact]
    public async Task HandleAsync_DoesNotCutAnEmojiInHalfWhenShorteningTheTitle()
    {
        // Arrange: the emoji (two UTF-16 chars) straddles the cut at character 149
        var prompt = new string('a', 148) + "🎉" + new string('b', 50);

        // Act
        await CreateHandler().HandleAsync(Command(prompt));

        // Assert
        var title = _chats.Chats.Single().Title;
        title.Should().Be(new string('a', 148) + "…");
        title.Length.Should().BeLessThanOrEqualTo(150);
        title.Any(char.IsSurrogate).Should().BeFalse();
    }

    // --- an existing chat --------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_WithAChat_ContinuesItAndSendsTheWholeHistoryToTheModel()
    {
        // Arrange
        var handler = CreateHandler();
        var first = await handler.HandleAsync(Command("A post about coffee"));
        _model.Reply = FakeChatClient.PostJson("Shorter", "Shorter body");

        // Act
        var second = await handler.HandleAsync(Command("Make it shorter", first.ChatId));

        // Assert
        second.ChatId.Should().Be(first.ChatId);
        second.Title.Should().Be("Shorter");
        second.Body.Should().Be("Shorter body");

        var chat = _chats.Chats.Should().ContainSingle().Subject;
        chat.Messages.OrderBy(m => m.Sequence).Select(m => (m.Sequence, m.Role)).Should().Equal(
            (1, ChatMessageRole.System),
            (2, ChatMessageRole.User),
            (3, ChatMessageRole.Assistant),
            (4, ChatMessageRole.User),
            (5, ChatMessageRole.Assistant));
        _chats.UpdateCalls.Should().Be(1);

        _model.Requests.Last().Messages.Select(m => (m.Role, m.Text)).Should().Equal(
            (ChatRole.System, PostGenerationPrompt.Instructions),
            (ChatRole.User, "A post about coffee"),
            (ChatRole.Assistant, FakeChatClient.PostJson(FakeChatClient.DefaultTitle, FakeChatClient.DefaultBody)),
            (ChatRole.User, "Make it shorter"));
    }

    [Fact]
    public async Task HandleAsync_WithAnEmptyPostGenerationChat_StoresTheInstructionsFirstAndKeepsTheTitle()
    {
        // Arrange
        var chat = _chats.Seed(_userId, ChatType.PostContentGeneration, "Prepared chat");

        // Act
        await CreateHandler().HandleAsync(Command(chatId: chat.Id));

        // Assert
        chat.Title.Should().Be("Prepared chat");
        chat.Messages.OrderBy(m => m.Sequence).Select(m => m.Role).Should().Equal(
            ChatMessageRole.System,
            ChatMessageRole.User,
            ChatMessageRole.Assistant);
        _chats.UpdateCalls.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WithAChatThatHoldsOnlyPrompts_AddsTheInstructionsToTheRequestButNotToTheHistory()
    {
        // Arrange
        var chat = _chats.Seed(_userId, ChatType.PostContentGeneration);
        chat.AddUserMessage("It is about coffee", Now.AddMinutes(-5));

        // Act
        await CreateHandler().HandleAsync(Command("Write it for Instagram", chat.Id));

        // Assert
        _model.Requests.Single().Messages.Select(m => (m.Role, m.Text)).Should().Equal(
            (ChatRole.System, PostGenerationPrompt.Instructions),
            (ChatRole.User, "It is about coffee"),
            (ChatRole.User, "Write it for Instagram"));
        chat.Messages.Select(m => m.Role).Should().NotContain(ChatMessageRole.System);
        chat.Messages.Should().HaveCount(3);
    }

    [Fact]
    public async Task HandleAsync_SendsTheHistoryInTheOrderOfTheSequence()
    {
        // Arrange
        var chat = _chats.Seed(_userId, ChatType.PostContentGeneration);
        chat.AddSystemMessage("Instructions", Now);
        chat.AddUserMessage("First", Now);
        chat.AddAssistantMessage(FakeChatClient.PostJson("T", "B"), Now);

        // Act
        await CreateHandler().HandleAsync(Command("Second", chat.Id));

        // Assert
        _model.Requests.Single().Messages.Select(m => m.Text).Should().Equal(
            "Instructions",
            "First",
            FakeChatClient.PostJson("T", "B"),
            "Second");
    }

    // --- rejected requests -------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_WhenChatDoesNotExist_ThrowsWithoutCallingTheModel()
    {
        // Act
        var act = () => CreateHandler().HandleAsync(Command(chatId: Guid.NewGuid()));

        // Assert
        await act.Should().ThrowAsync<ChatNotFoundException>();
        _model.Requests.Should().BeEmpty();
        _chats.UpdateCalls.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WhenChatBelongsToAnotherUser_ThrowsAndStoresNothing()
    {
        // Arrange
        var chat = _chats.Seed(_userId, ChatType.PostContentGeneration);

        // Act
        var act = () => CreateHandler().HandleAsync(Command(chatId: chat.Id, userId: Guid.NewGuid()));

        // Assert
        await act.Should().ThrowAsync<ChatNotOwnedException>();
        _model.Requests.Should().BeEmpty();
        chat.Messages.Should().BeEmpty();
        _chats.UpdateCalls.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WhenChatIsNotAPostGenerationChat_ThrowsWithoutCallingTheModel()
    {
        // Arrange
        var chat = _chats.Seed(_userId, ChatType.AskAgent);

        // Act
        var act = () => CreateHandler().HandleAsync(Command(chatId: chat.Id));

        // Assert
        var exception = await act.Should().ThrowAsync<ChatTypeNotSupportedException>();
        exception.Which.Code.Should().Be("chat_type_not_supported");
        _model.Requests.Should().BeEmpty();
        chat.Messages.Should().BeEmpty();
        _chats.UpdateCalls.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WhenChatIsArchived_ThrowsWithoutCallingTheModel()
    {
        // Arrange
        var chat = _chats.Seed(_userId, ChatType.PostContentGeneration);
        chat.Archive();

        // Act
        var act = () => CreateHandler().HandleAsync(Command(chatId: chat.Id));

        // Assert
        await act.Should().ThrowAsync<ChatArchivedException>();
        _model.Requests.Should().BeEmpty();
        _chats.UpdateCalls.Should().Be(0);
    }

    // --- failed generation -------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_WhenTheModelFails_ThrowsAndDoesNotStoreTheNewChat()
    {
        // Arrange
        _model.Failure = new GoogleAiStudioIntegrationException("Google AI Studio returned 500.");

        // Act
        var act = () => CreateHandler().HandleAsync(Command());

        // Assert
        await act.Should().ThrowAsync<GoogleAiStudioIntegrationException>();
        _chats.Chats.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenTheModelFailsInAnExistingChat_DoesNotPersistIt()
    {
        // Arrange
        var chat = _chats.Seed(_userId, ChatType.PostContentGeneration);
        _model.Failure = new GoogleAiStudioIntegrationException("Google AI Studio returned 500.");

        // Act
        var act = () => CreateHandler().HandleAsync(Command(chatId: chat.Id));

        // Assert
        await act.Should().ThrowAsync<GoogleAiStudioIntegrationException>();
        _chats.UpdateCalls.Should().Be(0);
    }

    [Theory]
    [InlineData("I am sorry, I cannot help with that.")]
    [InlineData("""{"title": "Only a title"}""")]
    public async Task HandleAsync_WhenTheModelAnswersWithoutAPost_ThrowsAndDoesNotStoreTheNewChat(string reply)
    {
        // Arrange
        _model.Reply = reply;

        // Act
        var act = () => CreateHandler().HandleAsync(Command());

        // Assert
        await act.Should().ThrowAsync<InvalidGeneratedPostException>();
        _chats.Chats.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenTheModelAnswersWithoutAPostInAnExistingChat_DoesNotPersistIt()
    {
        // Arrange
        var chat = _chats.Seed(_userId, ChatType.PostContentGeneration);
        _model.Reply = "not a post";

        // Act
        var act = () => CreateHandler().HandleAsync(Command(chatId: chat.Id));

        // Assert
        await act.Should().ThrowAsync<InvalidGeneratedPostException>();
        _chats.UpdateCalls.Should().Be(0);
    }

    // --- the answer --------------------------------------------------------------------------

    [Fact]
    public async Task HandleAsync_StoresTheRawAnswerAndReturnsTheParsedPost()
    {
        // Arrange
        _model.Reply = "```json\n{\"title\": \"  Wrapped  \", \"body\": \"Body\"}\n```";

        // Act
        var result = await CreateHandler().HandleAsync(Command());

        // Assert
        result.Title.Should().Be("Wrapped");
        result.Body.Should().Be("Body");
        _chats.Chats.Single().Messages.Single(m => m.Role == ChatMessageRole.Assistant).Content
            .Should().Be("```json\n{\"title\": \"  Wrapped  \", \"body\": \"Body\"}\n```");
    }

    [Fact]
    public async Task HandleAsync_WhenTheProviderReportsNoUsage_StoresNoTokens()
    {
        // Arrange
        _model.InputTokens = null;
        _model.OutputTokens = null;

        // Act
        var result = await CreateHandler().HandleAsync(Command());

        // Assert
        result.InputTokens.Should().BeNull();
        result.OutputTokens.Should().BeNull();
        var answer = _chats.Chats.Single().Messages.Single(m => m.Role == ChatMessageRole.Assistant);
        answer.InputTokens.Should().BeNull();
        answer.OutputTokens.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenTheProviderNamesNoModel_StoresNoModel()
    {
        // Arrange
        _model.Model = null;

        // Act
        var result = await CreateHandler().HandleAsync(Command());

        // Assert
        result.Model.Should().BeNull();
        _chats.Chats.Single().Messages.Single(m => m.Role == ChatMessageRole.Assistant).Model.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenTheModelNameIsTooLongToStore_StoresItShortenedInsteadOfLosingTheAnswer()
    {
        // Arrange
        _model.Model = "vendor/" + new string('m', 200);

        // Act
        var result = await CreateHandler().HandleAsync(Command());

        // Assert
        result.Model.Should().Be("vendor/" + new string('m', 200));
        var answer = _chats.Chats.Single().Messages.Single(m => m.Role == ChatMessageRole.Assistant);
        answer.Model.Should().HaveLength(100).And.StartWith("vendor/mmm");
    }
}
