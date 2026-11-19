using FluentAssertions;
using Microsoft.Extensions.AI;
using StarviaBackend.Modules.Ai.Application.AskAgent;
using StarviaBackend.Modules.Ai.Application.AskAgent.Commands.AskAgent;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Core.Chats.Exceptions;
using StarviaBackend.Modules.Ai.Infrastructure.GoogleAiStudio;
using StarviaBackend.Modules.Ai.Tests.Unit.Fakes;
using StarviaBackend.Modules.Brands.Integrations;

namespace StarviaBackend.Modules.Ai.Tests.Unit.Handlers;

public sealed class AskAgentHandlerTests
{
    private static readonly DateTime Now = new(2026, 10, 30, 12, 0, 0, DateTimeKind.Utc);

    private readonly InMemoryChatRepository _chats = new();
    private readonly FakeBrandsClient _brands = new();
    private readonly FakeChatClient _model = new();
    private readonly FakeClock _clock = new(Now);
    private readonly Guid _userId = Guid.NewGuid();

    public AskAgentHandlerTests()
    {
        _model.Reply = "Try shorter hooks on LinkedIn and lead with a question.";
    }

    private AskAgentHandler CreateHandler(Guid? actingUserId = null) =>
        new(_chats, TestAccess.For(actingUserId ?? _userId), _brands, _model, _clock);

    private AskAgentCommand Command(
        string prompt = "How should I open a LinkedIn post?",
        Guid? chatId = null,
        string? title = null,
        Guid? brandProfileId = null,
        Guid? userId = null) =>
        new(new AskAgentRequest(prompt, chatId, title, brandProfileId), userId ?? _userId);

    [Fact]
    public async Task HandleAsync_ReturnsTheAnswerAndTheDetailsOfTheCall()
    {
        // Act
        var result = await CreateHandler().HandleAsync(Command());

        // Assert
        var chat = _chats.Chats.Should().ContainSingle().Subject;
        result.ChatId.Should().Be(chat.Id);
        result.MessageId.Should().Be(chat.Messages.Last().Id);
        result.Answer.Should().Be(_model.Reply);
        result.Model.Should().Be("fake/model-1");
        result.InputTokens.Should().Be(42);
        result.OutputTokens.Should().Be(17);
    }

    [Fact]
    public async Task HandleAsync_StartsAnAskAgentChatWithDefaultTitle()
    {
        // Act
        await CreateHandler().HandleAsync(Command());

        // Assert
        var chat = _chats.Chats.Should().ContainSingle().Subject;
        chat.UserId.Should().Be(_userId);
        chat.Type.Should().Be(ChatType.AskAgent);
        chat.PostId.Should().BeNull();
        chat.Title.Should().Be(AskAgentPrompt.DefaultChatTitle);
        chat.LastMessageAt.Should().Be(Now);

        var messages = chat.Messages.OrderBy(m => m.Sequence).ToList();
        messages.Select(m => (m.Sequence, m.Role)).Should().Equal(
            (1, ChatMessageRole.System),
            (2, ChatMessageRole.User),
            (3, ChatMessageRole.Assistant));
        messages[0].Content.Should().Be(AskAgentPrompt.Instructions);
        messages[1].Content.Should().Be("How should I open a LinkedIn post?");
        messages[2].Content.Should().Be(_model.Reply);
    }

    [Fact]
    public async Task HandleAsync_UsesCustomTitleWhenStartingAChat()
    {
        // Act
        await CreateHandler().HandleAsync(Command(title: "  Strategie Q1  "));

        // Assert
        _chats.Chats.Single().Title.Should().Be("Strategie Q1");
    }

    [Fact]
    public async Task HandleAsync_SendsInstructionsAndPromptWithoutJsonResponseFormat()
    {
        // Act
        await CreateHandler().HandleAsync(Command("Give me 3 hooks"));

        // Assert
        var request = _model.Requests.Should().ContainSingle().Subject;
        request.Options.Should().NotBeNull();
        request.Options!.ResponseFormat.Should().BeNull();
        request.Options.MaxOutputTokens.Should().Be(AskAgentPrompt.MaxTokens);
        request.Messages.Select(m => (m.Role, m.Text)).Should().Equal(
            (ChatRole.System, AskAgentPrompt.Instructions),
            (ChatRole.User, "Give me 3 hooks"));
    }

    [Fact]
    public async Task HandleAsync_ContinuesAnExistingChatAndSendsHistoryToTheModel()
    {
        // Arrange
        var handler = CreateHandler();
        var first = await handler.HandleAsync(Command("First question"));
        _model.Requests.Clear();
        _model.Reply = "Here is a follow-up idea.";

        // Act
        var second = await handler.HandleAsync(Command("Make it punchier", first.ChatId));

        // Assert
        second.ChatId.Should().Be(first.ChatId);
        _chats.Chats.Should().ContainSingle();
        _chats.UpdateCalls.Should().Be(1);

        var followUp = _model.Requests.Should().ContainSingle().Subject;
        followUp.Messages.Select(m => (m.Role, m.Text)).Should().Equal(
            (ChatRole.System, AskAgentPrompt.Instructions),
            (ChatRole.User, "First question"),
            (ChatRole.Assistant, "Try shorter hooks on LinkedIn and lead with a question."),
            (ChatRole.User, "Make it punchier"));

        var chat = _chats.Chats.Single();
        chat.Messages.Select(m => m.Role).Should().Equal(
            ChatMessageRole.System,
            ChatMessageRole.User,
            ChatMessageRole.Assistant,
            ChatMessageRole.User,
            ChatMessageRole.Assistant);
    }

    [Fact]
    public async Task HandleAsync_WhenChatIsPostGeneration_ThrowsWithoutCallingTheModel()
    {
        // Arrange
        var chat = _chats.Seed(_userId, ChatType.PostContentGeneration, "Post chat");

        // Act
        var act = () => CreateHandler().HandleAsync(Command(chatId: chat.Id));

        // Assert
        await act.Should().ThrowAsync<ChatTypeNotSupportedException>();
        _model.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenChatDoesNotExist_ThrowsWithoutCallingTheModel()
    {
        // Act
        var act = () => CreateHandler().HandleAsync(Command(chatId: Guid.NewGuid()));

        // Assert
        await act.Should().ThrowAsync<ChatNotFoundException>();
        _model.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenCallerDoesNotOwnTheChat_ThrowsWithoutCallingTheModel()
    {
        // Arrange
        var chat = _chats.Seed(Guid.NewGuid(), ChatType.AskAgent, "Someone else");

        // Act
        var act = () => CreateHandler().HandleAsync(Command(chatId: chat.Id));

        // Assert
        await act.Should().ThrowAsync<ChatAccessDeniedException>();
        _model.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenBrandProfileIsGiven_LoadsItAndSendsBrandContext()
    {
        // Arrange
        var brandId = Guid.NewGuid();

        // Act
        await CreateHandler().HandleAsync(Command(brandProfileId: brandId));

        // Assert
        _brands.Calls.Should().ContainSingle().Which.BrandProfileId.Should().Be(brandId);
        var brandContext = _model.Requests.Single().Messages[1];
        brandContext.Role.Should().Be(ChatRole.System);
        brandContext.Text.Should().Contain("BRAND PROFILE CONTEXT");
        brandContext.Text.Should().Contain("Acme");
    }

    [Fact]
    public async Task HandleAsync_WhenBrandProfileIsUnknown_ThrowsWithoutCallingTheModel()
    {
        // Arrange
        var brandId = Guid.NewGuid();
        _brands.UnknownProfileIds.Add(brandId);

        // Act
        var act = () => CreateHandler().HandleAsync(Command(brandProfileId: brandId));

        // Assert
        await act.Should().ThrowAsync<BrandsIntegrationException>();
        _model.Requests.Should().BeEmpty();
        _chats.Chats.Should().BeEmpty();
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

    [Fact]
    public async Task HandleAsync_WhenTheModelFailsForAnExistingChat_DoesNotCallUpdate()
    {
        // Arrange
        var handler = CreateHandler();
        var first = await handler.HandleAsync(Command("First question"));
        _model.Failure = new GoogleAiStudioIntegrationException("Google AI Studio returned 500.");

        // Act
        var act = () => handler.HandleAsync(Command("Second question", first.ChatId));

        // Assert
        await act.Should().ThrowAsync<GoogleAiStudioIntegrationException>();
        _chats.UpdateCalls.Should().Be(0);
    }
}
