using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.AI;
using StarviaBackend.Modules.Ai.Application.PostGeneration;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Infrastructure.GoogleAiStudio;

namespace StarviaBackend.Modules.Ai.Tests.Integration.PostGeneration;

public sealed class PostGenerationEndpointsTests(AiApp app) : AiIntegrationTest(app)
{
    private FakeChatClient TextModel => App.TextModel;
    private FakePostsClient Posts => App.Posts;

    private static Task<HttpResponseMessage> GenerateAsync(HttpClient client, string prompt, Guid postId) =>
        client.PostAsJsonAsync(GeneratePostUrl, new { prompt, postId });

    private static async Task<GeneratedPostResponse> GenerateOkAsync(HttpClient client, string prompt, Guid postId)
    {
        var response = await GenerateAsync(client, prompt, postId);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<GeneratedPostResponse>())!;
    }

    // --- a new generation for a post ---------------------------------------------------------

    [Fact]
    public async Task Generate_returns_the_post_body_written_by_the_model()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = Guid.NewGuid();

        // Act
        var post = await GenerateOkAsync(client, "A post about our new coffee blend", postId);

        // Assert
        post.Body.Should().Be(FakeChatClient.DefaultBody);
        post.Model.Should().Be(FakeChatClient.DefaultModel);
        post.InputTokens.Should().Be(42);
        post.OutputTokens.Should().Be(17);
        post.ChatId.Should().NotBeEmpty();
        post.MessageId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Generate_starts_a_post_generation_chat_named_after_the_post()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = Guid.NewGuid();

        // Act
        var post = await GenerateOkAsync(client, "  A post about our\nnew coffee blend  ", postId);

        // Assert
        var chat = await GetChatAsync(client, post.ChatId);
        chat.Type.Should().Be(ChatType.PostContentGeneration);
        chat.Status.Should().Be(ChatStatus.Active);
        chat.PostId.Should().Be(postId);
        chat.Title.Should().Be($"Generowanie postu: Post {postId}");
        chat.LastMessageAt.Should().NotBeNull();

        chat.Messages.Select(m => m.Role).Should().Equal(ChatMessageRole.System, ChatMessageRole.User, ChatMessageRole.Assistant);
        chat.Messages.Select(m => m.Sequence).Should().Equal(1, 2, 3);
        chat.Messages[0].Content.Should().Be(PostGenerationPrompt.Instructions);
        chat.Messages[1].Content.Should().Be("  A post about our\nnew coffee blend  ");

        var answer = chat.Messages[2];
        answer.Id.Should().Be(post.MessageId);
        answer.Content.Should().Be(TextModel.Reply);
        answer.Model.Should().Be(FakeChatClient.DefaultModel);
        answer.InputTokens.Should().Be(42);
        answer.OutputTokens.Should().Be(17);
    }

    [Fact]
    public async Task Generate_asks_the_model_with_instructions_current_post_and_prompt()
    {
        // Arrange
        var postId = Guid.NewGuid();

        // Act
        await GenerateOkAsync(CreateUserClient(), "A post about coffee", postId);

        // Assert
        var request = TextModel.Requests.Should().ContainSingle().Subject;
        request.Options!.ResponseFormat.Should().Be(ChatResponseFormat.Json);
        request.Options.MaxOutputTokens.Should().Be(PostGenerationPrompt.MaxTokens);
        request.Messages.Select(m => (m.Role, m.Text)).Should().Equal(
            (ChatRole.System, PostGenerationPrompt.Instructions),
            (ChatRole.System, PostGenerationPrompt.CurrentContentOf($"Body of {postId}")),
            (ChatRole.User, "A post about coffee"));
    }

    [Fact]
    public async Task Generate_for_two_posts_creates_two_independent_chats()
    {
        // Arrange
        var client = CreateUserClient();
        var firstPostId = Guid.NewGuid();
        var secondPostId = Guid.NewGuid();

        // Act
        var first = await GenerateOkAsync(client, "Same prompt", firstPostId);
        var second = await GenerateOkAsync(client, "Same prompt", secondPostId);

        // Assert
        first.ChatId.Should().NotBe(second.ChatId);
        (await GetChatAsync(client, first.ChatId)).PostId.Should().Be(firstPostId);
        (await GetChatAsync(client, second.ChatId)).PostId.Should().Be(secondPostId);
        (await GetChatAsync(client, first.ChatId)).Messages.Should().HaveCount(3);
        (await GetChatAsync(client, second.ChatId)).Messages.Should().HaveCount(3);
    }

    // --- refining the same post --------------------------------------------------------------

    [Fact]
    public async Task Generate_for_the_same_post_reuses_the_chat_and_appends_the_exchange()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = Guid.NewGuid();
        var first = await GenerateOkAsync(client, "A post about coffee", postId);
        TextModel.Reply = FakeChatClient.PostJson("Shorter title", "Shorter body");

        // Act
        var second = await GenerateOkAsync(client, "Make it shorter", postId);

        // Assert
        second.ChatId.Should().Be(first.ChatId);
        second.MessageId.Should().NotBe(first.MessageId);
        second.Body.Should().Be("Shorter body");

        var chat = await GetChatAsync(client, first.ChatId);
        chat.PostId.Should().Be(postId);
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
    public async Task Generate_for_the_same_post_does_not_send_chat_history_to_the_model()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = Guid.NewGuid();
        await GenerateOkAsync(client, "A post about coffee", postId);

        // Act
        await GenerateOkAsync(client, "Make it shorter", postId);

        // Assert
        var followUp = TextModel.Requests.Should().HaveCount(2).And.Subject.Last();
        followUp.Messages.Select(m => (m.Role, m.Text)).Should().Equal(
            (ChatRole.System, PostGenerationPrompt.Instructions),
            (ChatRole.System, PostGenerationPrompt.CurrentContentOf($"Body of {postId}")),
            (ChatRole.User, "Make it shorter"));
    }

    [Fact]
    public async Task Generate_for_an_archived_post_chat_returns_chat_archived_and_does_not_call_the_model()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = Guid.NewGuid();
        var first = await GenerateOkAsync(client, "A post about coffee", postId);
        TextModel.Requests.Clear();
        (await client.PutAsJsonAsync($"{ChatsUrl}/{first.ChatId}", new { title = $"Generowanie postu: Post {postId}", status = ChatStatus.Archived }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act
        var response = await GenerateAsync(client, "Make it shorter", postId);

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "chat_archived");
        TextModel.Requests.Should().BeEmpty();
        (await GetChatAsync(client, first.ChatId)).Messages.Should().HaveCount(3);
    }

    // --- post access -------------------------------------------------------------------------

    [Fact]
    public async Task Generate_for_an_unknown_post_returns_posts_integration_failure()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = Guid.NewGuid();
        Posts.UnknownPostIds.Add(postId);

        // Act
        var response = await GenerateAsync(client, "A post about coffee", postId);

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "posts_integration_failed");
        TextModel.Requests.Should().BeEmpty();
        var chats = await client.GetFromJsonAsync<ChatPageResponse>(ChatsUrl);
        chats!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Generate_for_someone_elses_post_returns_post_not_owned()
    {
        // Arrange
        var postId = Guid.NewGuid();
        Posts.ForeignPostIds.Add(postId);

        // Act
        var response = await GenerateAsync(CreateUserClient(), "A post about coffee", postId);

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "post_not_owned");
        TextModel.Requests.Should().BeEmpty();
    }

    // --- failures ----------------------------------------------------------------------------

    [Fact]
    public async Task Generate_when_google_ai_studio_fails_returns_the_integration_error_and_stores_no_chat()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = Guid.NewGuid();
        TextModel.Failure = new GoogleAiStudioIntegrationException("Google AI Studio returned 500.");

        // Act
        var response = await GenerateAsync(client, "A post about coffee", postId);

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
        var postId = Guid.NewGuid();

        // Act
        var response = await GenerateAsync(client, "A post about coffee", postId);

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "google_ai_studio_integration_failed");
        (await response.Content.ReadAsStringAsync()).Should().Contain("API key is not configured");
        TextModel.Requests.Should().BeEmpty("the fake is not in use");
        var chats = await client.GetFromJsonAsync<ChatPageResponse>(ChatsUrl);
        chats!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Generate_when_google_ai_studio_fails_for_an_existing_post_keeps_history_unchanged_and_a_retry_works()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = Guid.NewGuid();
        var first = await GenerateOkAsync(client, "A post about coffee", postId);
        TextModel.Failure = new GoogleAiStudioIntegrationException("Google AI Studio returned 500.");

        // Act
        var failed = await GenerateAsync(client, "Make it shorter", postId);
        TextModel.Failure = null;
        var retried = await GenerateOkAsync(client, "Make it shorter", postId);

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
    [InlineData("{\"title\": \"Title\", \"body\": \"   \"}")]
    [InlineData("{\"title\": \"Broken")]
    public async Task Generate_when_the_model_does_not_return_a_post_returns_generated_post_invalid_and_stores_nothing(string reply)
    {
        // Arrange
        var client = CreateUserClient();
        var postId = Guid.NewGuid();
        TextModel.Reply = reply;

        // Act
        var response = await GenerateAsync(client, "A post about coffee", postId);

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "generated_post_invalid");
        var chats = await client.GetFromJsonAsync<ChatPageResponse>(ChatsUrl);
        chats!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Generate_accepts_a_post_wrapped_in_a_code_block()
    {
        // Arrange
        var postId = Guid.NewGuid();
        TextModel.Reply = "```json\n" + FakeChatClient.PostJson("Wrapped", "Still a post") + "\n```";

        // Act
        var post = await GenerateOkAsync(CreateUserClient(), "A post about coffee", postId);

        // Assert
        post.Body.Should().Be("Still a post");
    }

    // --- validation --------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Generate_with_a_blank_prompt_returns_bad_request(string prompt)
    {
        // Act
        var response = await GenerateAsync(CreateUserClient(), prompt, Guid.NewGuid());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        TextModel.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Generate_without_a_prompt_returns_bad_request()
    {
        // Act
        var response = await CreateUserClient().PostAsJsonAsync(GeneratePostUrl, new { postId = Guid.NewGuid() });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        TextModel.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Generate_accepts_the_maximum_prompt_length_and_rejects_longer_prompts()
    {
        // Arrange
        var client = CreateUserClient();
        var postId = Guid.NewGuid();

        // Act
        var atLimit = await GenerateAsync(client, new string('a', 4000), postId);
        var overLimit = await GenerateAsync(client, new string('a', 4001), Guid.NewGuid());

        // Assert
        atLimit.StatusCode.Should().Be(HttpStatusCode.OK);
        overLimit.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        TextModel.Requests.Should().ContainSingle();
    }

    [Fact]
    public async Task Generate_with_an_empty_post_id_returns_bad_request()
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
        var response = await GenerateAsync(App.CreateClient(), "A post about coffee", Guid.NewGuid());

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
        string Body,
        string Model,
        int? InputTokens,
        int? OutputTokens);
}
