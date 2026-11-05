using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.AI;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Infrastructure.GoogleAiStudio;

namespace StarviaBackend.Modules.Ai.Tests.Integration.PostGeneration;

public sealed class PostGenerationEndpointsTests(AiApp app) : AiIntegrationTest(app)
{
    private FakeChatClient TextModel => App.TextModel;

    private static Task<HttpResponseMessage> GenerateAsync(HttpClient client, string prompt, Guid? chatId = null) =>
        client.PostAsJsonAsync(GeneratePostUrl, new { prompt, chatId });

    private static async Task<GeneratedPostResponse> GenerateOkAsync(HttpClient client, string prompt, Guid? chatId = null)
    {
        var response = await GenerateAsync(client, prompt, chatId);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<GeneratedPostResponse>())!;
    }

    // --- a new chat --------------------------------------------------------------------------

    [Fact]
    public async Task Generate_returns_the_post_written_by_the_model()
    {
        // Arrange
        var client = CreateUserClient();

        // Act
        var post = await GenerateOkAsync(client, "A post about our new coffee blend");

        // Assert
        post.Title.Should().Be(FakeChatClient.DefaultTitle);
        post.Body.Should().Be(FakeChatClient.DefaultBody);
        post.Model.Should().Be(FakeChatClient.DefaultModel);
        post.InputTokens.Should().Be(42);
        post.OutputTokens.Should().Be(17);
        post.ChatId.Should().NotBeEmpty();
        post.MessageId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Generate_without_a_chat_starts_a_post_generation_chat_with_the_whole_exchange()
    {
        // Arrange
        var client = CreateUserClient();

        // Act
        var post = await GenerateOkAsync(client, "  A post about our\nnew coffee blend  ");

        // Assert
        var chat = await GetChatAsync(client, post.ChatId);
        chat.Type.Should().Be(ChatType.PostContentGeneration);
        chat.Status.Should().Be(ChatStatus.Active);
        chat.Title.Should().Be("A post about our new coffee blend");
        chat.LastMessageAt.Should().NotBeNull();

        chat.Messages.Select(m => m.Role).Should().Equal(ChatMessageRole.System, ChatMessageRole.User, ChatMessageRole.Assistant);
        chat.Messages.Select(m => m.Sequence).Should().Equal(1, 2, 3);
        chat.Messages[0].Content.Should().Contain("JSON");
        chat.Messages[1].Content.Should().Be("  A post about our\nnew coffee blend  ");

        var answer = chat.Messages[2];
        answer.Id.Should().Be(post.MessageId);
        answer.Content.Should().Be(TextModel.Reply);
        answer.Model.Should().Be(FakeChatClient.DefaultModel);
        answer.InputTokens.Should().Be(42);
        answer.OutputTokens.Should().Be(17);
    }

    [Fact]
    public async Task Generate_asks_the_model_for_a_json_post()
    {
        // Act
        await GenerateOkAsync(CreateUserClient(), "A post about coffee");

        // Assert
        var request = TextModel.Requests.Should().ContainSingle().Subject;
        request.Options!.ResponseFormat.Should().Be(ChatResponseFormat.Json);
        request.Options.MaxOutputTokens.Should().BePositive();
        request.Messages.Select(m => m.Role).Should().Equal(ChatRole.System, ChatRole.User);
        request.Messages[1].Text.Should().Be("A post about coffee");
    }

    [Fact]
    public async Task Generate_called_twice_without_a_chat_creates_two_independent_chats()
    {
        // Arrange
        var client = CreateUserClient();

        // Act
        var first = await GenerateOkAsync(client, "Same prompt");
        var second = await GenerateOkAsync(client, "Same prompt");

        // Assert
        first.ChatId.Should().NotBe(second.ChatId);
        (await GetChatAsync(client, first.ChatId)).Messages.Should().HaveCount(3);
        (await GetChatAsync(client, second.ChatId)).Messages.Should().HaveCount(3);
    }

    [Fact]
    public async Task Generate_shortens_a_long_prompt_to_fit_the_chat_title()
    {
        // Arrange
        var client = CreateUserClient();

        // Act
        var post = await GenerateOkAsync(client, new string('a', 400));

        // Assert
        var chat = await GetChatAsync(client, post.ChatId);
        chat.Title.Should().HaveLength(150).And.EndWith("…");
    }

    // --- an existing chat --------------------------------------------------------------------

    [Fact]
    public async Task Generate_with_a_chat_id_refines_the_post_in_the_same_chat()
    {
        // Arrange
        var client = CreateUserClient();
        var first = await GenerateOkAsync(client, "A post about coffee");
        TextModel.Reply = FakeChatClient.PostJson("Shorter title", "Shorter body");

        // Act
        var second = await GenerateOkAsync(client, "Make it shorter", first.ChatId);

        // Assert
        second.ChatId.Should().Be(first.ChatId);
        second.MessageId.Should().NotBe(first.MessageId);
        second.Title.Should().Be("Shorter title");
        second.Body.Should().Be("Shorter body");

        var chat = await GetChatAsync(client, first.ChatId);
        chat.Messages.Select(m => m.Role).Should().Equal(
            ChatMessageRole.System,
            ChatMessageRole.User,
            ChatMessageRole.Assistant,
            ChatMessageRole.User,
            ChatMessageRole.Assistant);
        chat.Messages.Select(m => m.Sequence).Should().Equal(1, 2, 3, 4, 5);
        chat.Messages[1].Content.Should().Be("A post about coffee");
        chat.Messages[3].Content.Should().Be("Make it shorter");
    }

    [Fact]
    public async Task Generate_with_a_chat_id_sends_the_whole_history_to_the_model()
    {
        // Arrange
        var client = CreateUserClient();
        var first = await GenerateOkAsync(client, "A post about coffee");

        // Act
        await GenerateOkAsync(client, "Make it shorter", first.ChatId);

        // Assert
        var followUp = TextModel.Requests.Should().HaveCount(2).And.Subject.Last();
        followUp.Messages.Select(m => m.Role).Should().Equal(
            ChatRole.System,
            ChatRole.User,
            ChatRole.Assistant,
            ChatRole.User);
        followUp.Messages[1].Text.Should().Be("A post about coffee");
        followUp.Messages[2].Text.Should().Be(FakeChatClient.PostJson(FakeChatClient.DefaultTitle, FakeChatClient.DefaultBody));
        followUp.Messages[3].Text.Should().Be("Make it shorter");
    }

    [Fact]
    public async Task Generate_can_use_an_empty_chat_created_through_the_chats_endpoint()
    {
        // Arrange
        var client = CreateUserClient();
        var chatId = await CreateChatAsync(client, ChatType.PostContentGeneration, "Prepared chat");

        // Act
        var post = await GenerateOkAsync(client, "A post about coffee", chatId);

        // Assert
        post.ChatId.Should().Be(chatId);
        var chat = await GetChatAsync(client, chatId);
        chat.Title.Should().Be("Prepared chat");
        chat.Messages.Select(m => m.Role).Should().Equal(ChatMessageRole.System, ChatMessageRole.User, ChatMessageRole.Assistant);
    }

    [Fact]
    public async Task Generate_in_a_chat_that_already_holds_prompts_keeps_them_and_adds_the_instructions_to_the_request_only()
    {
        // Arrange
        var client = CreateUserClient();
        var chatId = await CreateChatAsync(client, ChatType.PostContentGeneration);
        (await AddMessageAsync(client, chatId, "It is about coffee")).StatusCode.Should().Be(HttpStatusCode.Created);

        // Act
        await GenerateOkAsync(client, "Write it for Instagram", chatId);

        // Assert
        var request = TextModel.Requests.Should().ContainSingle().Subject;
        request.Messages.Select(m => m.Role).Should().Equal(
            ChatRole.System,
            ChatRole.User,
            ChatRole.User);

        var chat = await GetChatAsync(client, chatId);
        chat.Messages.Select(m => m.Role).Should().Equal(
            ChatMessageRole.User,
            ChatMessageRole.User,
            ChatMessageRole.Assistant);
    }

    [Fact]
    public async Task Generate_in_an_ask_agent_chat_returns_chat_type_not_supported_and_does_not_call_the_model()
    {
        // Arrange
        var client = CreateUserClient();
        var chatId = await CreateChatAsync(client, ChatType.AskAgent);

        // Act
        var response = await GenerateAsync(client, "A post about coffee", chatId);

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "chat_type_not_supported");
        TextModel.Requests.Should().BeEmpty();
        (await GetChatAsync(client, chatId)).Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task Generate_in_an_unknown_chat_returns_chat_not_found()
    {
        // Act
        var response = await GenerateAsync(CreateUserClient(), "A post about coffee", Guid.NewGuid());

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "chat_not_found");
        TextModel.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Generate_in_a_chat_of_another_user_returns_forbidden_and_stores_nothing()
    {
        // Arrange
        var owner = CreateUserClient();
        var chatId = await CreateChatAsync(owner, ChatType.PostContentGeneration);

        // Act
        var response = await GenerateAsync(CreateUserClient(), "A post about coffee", chatId);

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.Forbidden, "chat_access_denied");
        TextModel.Requests.Should().BeEmpty();
        (await GetChatAsync(owner, chatId)).Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task Generate_in_an_archived_chat_returns_chat_archived_and_does_not_call_the_model()
    {
        // Arrange
        var client = CreateUserClient();
        var chatId = await CreateChatAsync(client, ChatType.PostContentGeneration, "Done");
        (await client.PutAsJsonAsync($"{ChatsUrl}/{chatId}", new { title = "Done", status = ChatStatus.Archived }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act
        var response = await GenerateAsync(client, "A post about coffee", chatId);

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "chat_archived");
        TextModel.Requests.Should().BeEmpty();
        (await GetChatAsync(client, chatId)).Messages.Should().BeEmpty();
    }

    // --- failures ----------------------------------------------------------------------------

    [Fact]
    public async Task Generate_when_google_ai_studio_fails_returns_the_integration_error_and_stores_no_chat()
    {
        // Arrange
        var client = CreateUserClient();
        TextModel.Failure = new GoogleAiStudioIntegrationException("Google AI Studio returned 500.");

        // Act
        var response = await GenerateAsync(client, "A post about coffee");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "google_ai_studio_integration_failed");
        var chats = await client.GetFromJsonAsync<ChatPageResponse>(ChatsUrl);
        chats!.Items.Should().BeEmpty();
    }

    // The one test that runs the application's own Google AI Studio registration: a fresh checkout has no key, and the
    // endpoint must answer with a clean error (not a 500) while the rest of the application works.
    [Fact]
    public async Task Generate_without_a_google_ai_studio_key_returns_the_integration_error_and_stores_no_chat()
    {
        // Arrange
        App.UseRealChatClient = true;
        var client = CreateUserClient();

        // Act
        var response = await GenerateAsync(client, "A post about coffee");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "google_ai_studio_integration_failed");
        (await response.Content.ReadAsStringAsync()).Should().Contain("API key is not configured");
        TextModel.Requests.Should().BeEmpty("the fake is not in use");
        var chats = await client.GetFromJsonAsync<ChatPageResponse>(ChatsUrl);
        chats!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Generate_when_google_ai_studio_fails_in_an_existing_chat_keeps_its_history_unchanged_and_a_retry_works()
    {
        // Arrange
        var client = CreateUserClient();
        var first = await GenerateOkAsync(client, "A post about coffee");
        TextModel.Failure = new GoogleAiStudioIntegrationException("Google AI Studio returned 500.");

        // Act
        var failed = await GenerateAsync(client, "Make it shorter", first.ChatId);
        TextModel.Failure = null;
        var retried = await GenerateOkAsync(client, "Make it shorter", first.ChatId);

        // Assert
        await AssertErrorCodeAsync(failed, HttpStatusCode.BadRequest, "google_ai_studio_integration_failed");
        retried.ChatId.Should().Be(first.ChatId);
        var chat = await GetChatAsync(client, first.ChatId);
        chat.Messages.Select(m => m.Sequence).Should().Equal(1, 2, 3, 4, 5);
        chat.Messages.Count(m => m.Content == "Make it shorter").Should().Be(1);
    }

    [Theory]
    [InlineData("I am sorry, I cannot help with that.")]
    [InlineData("{\"title\": \"Only a title\"}")]
    [InlineData("{\"title\": \"\", \"body\": \"Only a body\"}")]
    [InlineData("{\"title\": \"Broken")]
    public async Task Generate_when_the_model_does_not_return_a_post_returns_generated_post_invalid_and_stores_nothing(string reply)
    {
        // Arrange
        var client = CreateUserClient();
        TextModel.Reply = reply;

        // Act
        var response = await GenerateAsync(client, "A post about coffee");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "generated_post_invalid");
        var chats = await client.GetFromJsonAsync<ChatPageResponse>(ChatsUrl);
        chats!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Generate_accepts_a_post_wrapped_in_a_code_block()
    {
        // Arrange
        TextModel.Reply = "```json\n" + FakeChatClient.PostJson("Wrapped", "Still a post") + "\n```";

        // Act
        var post = await GenerateOkAsync(CreateUserClient(), "A post about coffee");

        // Assert
        post.Title.Should().Be("Wrapped");
        post.Body.Should().Be("Still a post");
    }

    // --- validation --------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Generate_with_a_blank_prompt_returns_bad_request(string prompt)
    {
        // Act
        var response = await GenerateAsync(CreateUserClient(), prompt);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        TextModel.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Generate_without_a_prompt_returns_bad_request()
    {
        // Act
        var response = await CreateUserClient().PostAsJsonAsync(GeneratePostUrl, new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        TextModel.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Generate_accepts_the_maximum_prompt_length_and_rejects_longer_prompts()
    {
        // Arrange
        var client = CreateUserClient();

        // Act
        var atLimit = await GenerateAsync(client, new string('a', 4000));
        var overLimit = await GenerateAsync(client, new string('a', 4001));

        // Assert
        atLimit.StatusCode.Should().Be(HttpStatusCode.OK);
        overLimit.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        TextModel.Requests.Should().ContainSingle();
    }

    [Fact]
    public async Task Generate_with_an_empty_chat_id_returns_bad_request()
    {
        // Act
        var response = await GenerateAsync(CreateUserClient(), "A post about coffee", Guid.Empty);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        TextModel.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Generate_without_identity_returns_unauthorized()
    {
        // Act
        var response = await GenerateAsync(App.CreateClient(), "A post about coffee");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        TextModel.Requests.Should().BeEmpty();
    }

    // --- swagger -----------------------------------------------------------------------------

    [Fact]
    public async Task Swagger_document_lists_the_generate_post_endpoint()
    {
        // Act
        var response = await App.CreateClient().GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var operations = document.RootElement.GetProperty("paths").EnumerateObject()
            .Where(path => path.Name.StartsWith("/v1/ai/posts", StringComparison.Ordinal))
            .SelectMany(path => path.Value.EnumerateObject().Select(method => $"{method.Name.ToUpperInvariant()} {path.Name}"))
            .ToList();

        operations.Should().BeEquivalentTo(["POST /v1/ai/posts/generate"]);
    }

    private sealed record GeneratedPostResponse(
        Guid ChatId,
        Guid MessageId,
        string Title,
        string Body,
        string Model,
        int? InputTokens,
        int? OutputTokens);
}
