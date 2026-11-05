using FluentAssertions;
using Microsoft.Extensions.AI;
using StarviaBackend.Modules.Ai.Application.PostGeneration;
using StarviaBackend.Modules.Ai.Application.PostGeneration.Commands.GeneratePost;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Infrastructure.GoogleAiStudio;
using StarviaBackend.Modules.Ai.Tests.Unit.Fakes;
using StarviaBackend.Modules.Posts.Integrations;

namespace StarviaBackend.Modules.Ai.Tests.Unit.Handlers;

public sealed class GeneratePostHandlerTests
{
    private static readonly DateTime Now = new(2026, 10, 30, 12, 0, 0, DateTimeKind.Utc);

    private readonly InMemoryChatRepository _chats = new();
    private readonly FakePostsClient _posts = new();
    private readonly FakeChatClient _model = new();
    private readonly FakeClock _clock = new(Now);
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _postId = Guid.NewGuid();

    private GeneratePostHandler CreateHandler() =>
        new(_chats, _posts, _model, _clock);

    private GeneratePostCommand Command(string prompt = "A post about coffee", Guid? postId = null, Guid? userId = null) =>
        new(new GeneratePostRequest(prompt, postId ?? _postId), userId ?? _userId);

    [Fact]
    public async Task HandleAsync_LoadsThePostBeforeCallingTheModel()
    {
        // Act
        await CreateHandler().HandleAsync(Command());

        // Assert
        var call = _posts.Calls.Should().ContainSingle().Subject;
        call.UserId.Should().Be(_userId);
        call.PostIds.Should().Equal(_postId);
    }

    [Fact]
    public async Task HandleAsync_WhenPostDoesNotExist_ThrowsWithoutCallingTheModel()
    {
        // Arrange
        _posts.UnknownPostIds.Add(_postId);

        // Act
        var act = () => CreateHandler().HandleAsync(Command());

        // Assert
        await act.Should().ThrowAsync<PostsIntegrationException>();
        _model.Requests.Should().BeEmpty();
        _chats.Chats.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotOwnThePost_ThrowsWithoutCallingTheModel()
    {
        // Arrange
        _posts.ForeignPostIds.Add(_postId);

        // Act
        var act = () => CreateHandler().HandleAsync(Command());

        // Assert
        await act.Should().ThrowAsync<PostsIntegrationNotOwnerException>();
        _model.Requests.Should().BeEmpty();
        _chats.Chats.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ReturnsThePostAndTheDetailsOfTheAnswer()
    {
        // Act
        var result = await CreateHandler().HandleAsync(Command());

        // Assert
        var chat = _chats.Chats.Should().ContainSingle().Subject;
        result.ChatId.Should().Be(chat.Id);
        result.MessageId.Should().Be(chat.Messages.Last().Id);
        result.Body.Should().Be(FakeChatClient.DefaultBody);
        result.Model.Should().Be("fake/model-1");
        result.InputTokens.Should().Be(42);
        result.OutputTokens.Should().Be(17);
    }

    [Fact]
    public async Task HandleAsync_StoresTheWholeExchangeInANewPostGenerationChat()
    {
        // Act
        await CreateHandler().HandleAsync(Command("  A post about coffee  "));

        // Assert
        var chat = _chats.Chats.Should().ContainSingle().Subject;
        chat.UserId.Should().Be(_userId);
        chat.Type.Should().Be(ChatType.PostContentGeneration);
        chat.Status.Should().Be(ChatStatus.Active);
        chat.Title.Should().Be($"Generowanie postu: Post {_postId}");
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
    }

    [Fact]
    public async Task HandleAsync_SendsInstructionsCurrentPostAndPromptWithoutChatHistory()
    {
        // Act
        await CreateHandler().HandleAsync(Command("A post about coffee"));

        // Assert
        var request = _model.Requests.Should().ContainSingle().Subject;
        request.Options.Should().NotBeNull();
        request.Options!.ResponseFormat.Should().Be(ChatResponseFormat.Json);
        request.Options.MaxOutputTokens.Should().Be(PostGenerationPrompt.MaxTokens);
        request.Messages.Select(m => (m.Role, m.Text)).Should().Equal(
            (ChatRole.System, PostGenerationPrompt.Instructions),
            (ChatRole.System, PostGenerationPrompt.CurrentContentOf($"Body of {_postId}")),
            (ChatRole.User, "A post about coffee"));
    }

    [Fact]
    public async Task HandleAsync_DoesNotSendPreviousChatMessagesToTheModel()
    {
        // Arrange
        var handler = CreateHandler();
        await handler.HandleAsync(Command("First prompt"));
        _model.Requests.Clear();

        // Act — same post id would reuse chat history in the DB, but the model must not see it
        await handler.HandleAsync(Command("Second prompt"));

        // Assert
        _model.Requests.Should().ContainSingle().Subject.Messages.Select(m => m.Text).Should().Equal(
            PostGenerationPrompt.Instructions,
            PostGenerationPrompt.CurrentContentOf($"Body of {_postId}"),
            "Second prompt");
    }

    [Fact]
    public async Task HandleAsync_NamesTheChatAfterThePostTitle()
    {
        // Act
        await CreateHandler().HandleAsync(Command());

        // Assert
        _chats.Chats.Single().Title.Should().Be($"Generowanie postu: Post {_postId}");
    }

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
    public async Task HandleAsync_StoresTheRawAnswerAndReturnsTheParsedBody()
    {
        // Arrange
        _model.Reply = "```json\n{\"title\": \"  Wrapped  \", \"body\": \"Body\"}\n```";

        // Act
        var result = await CreateHandler().HandleAsync(Command());

        // Assert
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
