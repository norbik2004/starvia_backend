using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.AI;
using StarviaBackend.Modules.Ai.Application.AskAgent;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Infrastructure.GoogleAiStudio;

namespace StarviaBackend.Modules.Ai.Tests.Integration.AskAgent;

public sealed class AskAgentEndpointsTests : AiIntegrationTest
{
    private const string DefaultReply = "Lead with a question and keep the first line under 140 characters.";

    private FakeChatClient TextModel => App.TextModel;

    public AskAgentEndpointsTests(AiApp app) : base(app)
    {
        TextModel.Reply = DefaultReply;
    }

    private static Task<HttpResponseMessage> AskAsync(
        HttpClient client,
        string prompt,
        Guid? chatId = null,
        string? title = null) =>
        client.PostAsJsonAsync(AskAgentUrl, new { prompt, chatId, title });

    private static async Task<AskAgentResponse> AskOkAsync(
        HttpClient client,
        string prompt,
        Guid? chatId = null,
        string? title = null)
    {
        var response = await AskAsync(client, prompt, chatId, title);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<AskAgentResponse>())!;
    }

    [Fact]
    public async Task Ask_returns_the_answer_written_by_the_model()
    {
        // Arrange
        var client = CreateUserClient();

        // Act
        var result = await AskOkAsync(client, "How should I open a LinkedIn post?");

        // Assert
        result.Answer.Should().Be(TextModel.Reply);
        result.Model.Should().Be(FakeChatClient.DefaultModel);
        result.InputTokens.Should().Be(42);
        result.OutputTokens.Should().Be(17);
        result.ChatId.Should().NotBeEmpty();
        result.MessageId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Ask_starts_an_ask_agent_chat_with_the_default_title()
    {
        // Arrange
        var client = CreateUserClient();

        // Act
        var result = await AskOkAsync(client, "  How should I open a LinkedIn post?  ");

        // Assert
        var chat = await GetChatAsync(client, result.ChatId);
        chat.Type.Should().Be(ChatType.AskAgent);
        chat.Status.Should().Be(ChatStatus.Active);
        chat.PostId.Should().BeNull();
        chat.Title.Should().Be(AskAgentPrompt.DefaultChatTitle);
        chat.LastMessageAt.Should().NotBeNull();

        chat.Messages.Select(m => m.Role).Should().Equal(
            ChatMessageRole.System,
            ChatMessageRole.User,
            ChatMessageRole.Assistant);
        chat.Messages[0].Content.Should().Be(AskAgentPrompt.Instructions);
        chat.Messages[1].Content.Should().Be("  How should I open a LinkedIn post?  ");
        chat.Messages[2].Id.Should().Be(result.MessageId);
        chat.Messages[2].Content.Should().Be(TextModel.Reply);
    }

    [Fact]
    public async Task Ask_uses_a_custom_title_when_starting_a_chat()
    {
        // Arrange
        var client = CreateUserClient();

        // Act
        var result = await AskOkAsync(client, "Give me hooks", title: "Strategie Q1");

        // Assert
        (await GetChatAsync(client, result.ChatId)).Title.Should().Be("Strategie Q1");
    }

    [Fact]
    public async Task Ask_sends_instructions_and_prompt_without_json_response_format()
    {
        // Act
        await AskOkAsync(CreateUserClient(), "Give me 3 hooks");

        // Assert
        var request = TextModel.Requests.Should().ContainSingle().Subject;
        request.Options!.ResponseFormat.Should().BeNull();
        request.Options.MaxOutputTokens.Should().Be(AskAgentPrompt.MaxTokens);
        request.Messages.Select(m => (m.Role, m.Text)).Should().Equal(
            (ChatRole.System, AskAgentPrompt.Instructions),
            (ChatRole.User, "Give me 3 hooks"));
    }

    [Fact]
    public async Task Ask_with_the_same_chat_id_reuses_the_chat_and_sends_history()
    {
        // Arrange
        var client = CreateUserClient();
        var first = await AskOkAsync(client, "First question");
        TextModel.Reply = "Here is a follow-up.";

        // Act
        var second = await AskOkAsync(client, "Make it punchier", first.ChatId);

        // Assert
        second.ChatId.Should().Be(first.ChatId);
        second.Answer.Should().Be("Here is a follow-up.");

        var followUp = TextModel.Requests.Should().HaveCount(2).And.Subject.Last();
        followUp.Messages.Select(m => (m.Role, m.Text)).Should().Equal(
            (ChatRole.System, AskAgentPrompt.Instructions),
            (ChatRole.User, "First question"),
            (ChatRole.Assistant, DefaultReply),
            (ChatRole.User, "Make it punchier"));

        var chat = await GetChatAsync(client, first.ChatId);
        chat.Messages.Select(m => m.Role).Should().Equal(
            ChatMessageRole.System,
            ChatMessageRole.User,
            ChatMessageRole.Assistant,
            ChatMessageRole.User,
            ChatMessageRole.Assistant);
    }

    [Fact]
    public async Task Ask_for_a_post_generation_chat_returns_chat_type_not_supported()
    {
        // Arrange
        var client = CreateUserClient();
        var chatId = await CreateChatAsync(client, ChatType.PostContentGeneration, "Post chat");
        TextModel.Requests.Clear();

        // Act
        var response = await AskAsync(client, "Help me", chatId);

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "chat_type_not_supported");
        TextModel.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Ask_for_someone_elses_chat_returns_access_denied()
    {
        // Arrange
        var owner = CreateUserClient();
        var stranger = CreateUserClient();
        var chatId = await CreateChatAsync(owner, ChatType.AskAgent, "Private");

        // Act
        var response = await AskAsync(stranger, "Help me", chatId);

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.Forbidden, "chat_access_denied");
        TextModel.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Ask_for_an_archived_chat_returns_chat_archived()
    {
        // Arrange
        var client = CreateUserClient();
        var first = await AskOkAsync(client, "First question");
        TextModel.Requests.Clear();
        (await client.PutAsJsonAsync($"{ChatsUrl}/{first.ChatId}", new { title = AskAgentPrompt.DefaultChatTitle, status = ChatStatus.Archived }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act
        var response = await AskAsync(client, "Second question", first.ChatId);

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "chat_archived");
        TextModel.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Ask_when_google_ai_studio_fails_returns_the_integration_error_and_stores_no_chat()
    {
        // Arrange
        var client = CreateUserClient();
        TextModel.Failure = new GoogleAiStudioIntegrationException("Google AI Studio returned 500.");

        // Act
        var response = await AskAsync(client, "Help me");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "google_ai_studio_integration_failed");
        var chats = await client.GetFromJsonAsync<ChatPageResponse>(ChatsUrl);
        chats!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Ask_when_google_ai_studio_fails_for_an_existing_chat_keeps_history_and_a_retry_works()
    {
        // Arrange
        var client = CreateUserClient();
        var first = await AskOkAsync(client, "First question");
        TextModel.Failure = new GoogleAiStudioIntegrationException("Google AI Studio returned 500.");

        // Act
        var failed = await AskAsync(client, "Second question", first.ChatId);
        TextModel.Failure = null;
        TextModel.Reply = "Retry answer";
        var retried = await AskOkAsync(client, "Second question", first.ChatId);

        // Assert
        await AssertErrorCodeAsync(failed, HttpStatusCode.BadRequest, "google_ai_studio_integration_failed");
        retried.ChatId.Should().Be(first.ChatId);
        retried.Answer.Should().Be("Retry answer");
        var chat = await GetChatAsync(client, first.ChatId);
        chat.Messages.Select(m => m.Sequence).Should().Equal(1, 2, 3, 4, 5);
        chat.Messages.Count(m => m.Content == "Second question").Should().Be(1);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Ask_with_a_blank_prompt_returns_bad_request(string prompt)
    {
        // Act
        var response = await AskAsync(CreateUserClient(), prompt);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        TextModel.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Ask_accepts_the_maximum_prompt_length_and_rejects_longer_prompts()
    {
        // Arrange
        var client = CreateUserClient();

        // Act
        var atLimit = await AskAsync(client, new string('a', 8000));
        var overLimit = await AskAsync(client, new string('a', 8001));

        // Assert
        atLimit.StatusCode.Should().Be(HttpStatusCode.OK);
        overLimit.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        TextModel.Requests.Should().ContainSingle();
    }

    [Fact]
    public async Task Ask_without_identity_returns_unauthorized()
    {
        // Act
        var response = await AskAsync(App.CreateClient(), "Help me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        TextModel.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Swagger_document_lists_the_ask_agent_endpoint()
    {
        // Act
        var response = await App.CreateClient().GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var operations = document.RootElement.GetProperty("paths").EnumerateObject()
            .Where(path => path.Name.StartsWith("/v1/ai/agent", StringComparison.Ordinal))
            .SelectMany(path => path.Value.EnumerateObject().Select(method => $"{method.Name.ToUpperInvariant()} {path.Name}"))
            .ToList();

        operations.Should().BeEquivalentTo(["POST /v1/ai/agent/ask"]);
    }

    private sealed record AskAgentResponse(
        Guid ChatId,
        Guid MessageId,
        string Answer,
        string Model,
        int? InputTokens,
        int? OutputTokens);
}
