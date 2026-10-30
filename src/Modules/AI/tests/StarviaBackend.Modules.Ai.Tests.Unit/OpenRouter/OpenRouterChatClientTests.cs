using System.Net;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Ai.Infrastructure.OpenRouter;
using StarviaBackend.Modules.Ai.Tests.Unit.Fakes;

namespace StarviaBackend.Modules.Ai.Tests.Unit.OpenRouter;

/// <summary>
/// The OpenRouter integration is the OpenAI library behind <see cref="IChatClient"/> (registered by key, as texts
/// are written by Google AI Studio), so these tests go through the real registration and a fake HTTP handler: they
/// pin what goes on the wire, and how the library and our error translation behave with the answers OpenRouter
/// can give.
/// </summary>
public sealed class OpenRouterChatClientTests
{
    private const string ApiKey = "sk-or-test";

    private static readonly ChatMessage[] Conversation =
    [
        new(ChatRole.System, "Be brief."),
        new(ChatRole.User, "Write a post."),
        new(ChatRole.Assistant, "Here it is."),
        new(ChatRole.User, "Shorter."),
    ];

    /// <summary>Resolves the client the way a request would and asks the model once.</summary>
    private static async Task<ChatResponse> AskAsync(
        HttpMessageHandler handler,
        ChatOptions? options = null,
        IEnumerable<ChatMessage>? messages = null,
        string apiKey = ApiKey,
        string model = "vendor/default-model",
        string baseUrl = "https://openrouter.test/api/v1/",
        int timeoutSeconds = 120,
        CancellationToken cancellationToken = default)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["integrations:openrouter:BaseUrl"] = baseUrl,
                ["integrations:openrouter:ApiKey"] = apiKey,
                ["integrations:openrouter:Model"] = model,
                ["integrations:openrouter:TimeoutSeconds"] = timeoutSeconds.ToString(),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOpenRouter(configuration);

        // Swaps the network for the fake; the rest of the client's pipeline stays as registered. The registration
        // also limits the HttpClient to the configured timeout; lifting that here lets the tests see the timeout
        // of the OpenAI library itself, which is the one that would otherwise cut a long request at 100 seconds.
        services.AddHttpClient(OpenRouterExtensions.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .ConfigureHttpClient(client => client.Timeout = Timeout.InfiniteTimeSpan);

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var client = scope.ServiceProvider.GetRequiredKeyedService<IChatClient>(OpenRouterExtensions.ServiceKey);
        return await client.GetResponseAsync(messages ?? Conversation, options, cancellationToken);
    }

    private static object Completion(
        string content = "Hello!",
        string? model = "vendor/model-2025",
        object? usage = null) => new
    {
        id = "gen-1",
        model,
        choices = new[] { new { finish_reason = "stop", message = new { role = "assistant", content } } },
        usage = usage ?? new { prompt_tokens = 11, completion_tokens = 7, total_tokens = 18 },
    };

    private static StubHttpMessageHandler JsonBody(string body) =>
        new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json"),
        });

    /// <summary>Never answers; only the caller's token (or a timeout wired into it) can end the request.</summary>
    private sealed class HangingHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new InvalidOperationException("Unreachable: the delay only ends by cancellation.");
        }
    }

    // --- request -----------------------------------------------------------------------------

    [Fact]
    public async Task GetResponseAsync_PostsTheConversationWithTheBearerKey()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Completion());

        // Act
        await AskAsync(handler);

        // Assert
        var sent = handler.Requests.Should().ContainSingle().Subject;
        sent.Method.Should().Be(HttpMethod.Post);
        sent.Uri.Should().Be(new Uri("https://openrouter.test/api/v1/chat/completions"));
        sent.Authorization.Should().Be($"Bearer {ApiKey}");

        var messages = sent.Json.GetProperty("messages").EnumerateArray()
            .Select(m => (m.GetProperty("role").GetString(), m.GetProperty("content").GetString()))
            .ToList();
        messages.Should().Equal(
            ("system", "Be brief."),
            ("user", "Write a post."),
            ("assistant", "Here it is."),
            ("user", "Shorter."));
    }

    [Fact]
    public async Task GetResponseAsync_UsesTheConfiguredEndpointKeyAndModel_EvenWithoutATrailingSlash()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Completion());

        // Act
        await AskAsync(handler, apiKey: "sk-or-from-config", model: "vendor/configured-model", baseUrl: "https://openrouter.test/api/v1");

        // Assert
        var sent = handler.Requests.Should().ContainSingle().Subject;
        sent.Uri.Should().Be(new Uri("https://openrouter.test/api/v1/chat/completions"));
        sent.Authorization.Should().Be("Bearer sk-or-from-config");
        sent.Json.GetProperty("model").GetString().Should().Be("vendor/configured-model");
    }

    [Fact]
    public async Task GetResponseAsync_PrefersTheModelOfTheOptions()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Completion());

        // Act
        await AskAsync(handler, new ChatOptions { ModelId = "vendor/other-model" });

        // Assert
        handler.Requests.Single().Json.GetProperty("model").GetString().Should().Be("vendor/other-model");
    }

    [Fact]
    public async Task GetResponseAsync_SendsOnlyTheOptionsThatWereAskedFor()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Completion());

        // Act
        await AskAsync(handler);

        // Assert
        var json = handler.Requests.Single().Json;
        json.TryGetProperty("temperature", out _).Should().BeFalse();
        json.TryGetProperty("max_completion_tokens", out _).Should().BeFalse();
        json.TryGetProperty("response_format", out _).Should().BeFalse();
    }

    [Fact]
    public async Task GetResponseAsync_SendsTheRequestedOptions()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Completion());
        var options = new ChatOptions
        {
            ResponseFormat = ChatResponseFormat.Json,
            Temperature = 0.4f,
            MaxOutputTokens = 300,
        };

        // Act
        await AskAsync(handler, options);

        // Assert
        var json = handler.Requests.Single().Json;
        json.GetProperty("temperature").GetDouble().Should().BeApproximately(0.4, 0.0001);
        json.GetProperty("response_format").GetProperty("type").GetString().Should().Be("json_object");

        // The OpenAI library names the limit max_completion_tokens; OpenRouter documents it with the semantics of max_tokens.
        json.GetProperty("max_completion_tokens").GetInt32().Should().Be(300);
    }

    [Fact]
    public async Task GetResponseAsync_KeepsNonAsciiTextIntact()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Completion());
        var messages = new[] { new ChatMessage(ChatRole.User, "Zażółć gęślą jaźń ☕") };

        // Act
        await AskAsync(handler, messages: messages);

        // Assert
        handler.Requests.Single().Json.GetProperty("messages")[0].GetProperty("content").GetString()
            .Should().Be("Zażółć gęślą jaźń ☕");
    }

    // --- answer ------------------------------------------------------------------------------

    [Fact]
    public async Task GetResponseAsync_ReturnsTheAnswerTheModelAndTheTokenUsage()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Completion("Fresh coffee!"));

        // Act
        var response = await AskAsync(handler);

        // Assert
        response.Text.Should().Be("Fresh coffee!");
        response.ModelId.Should().Be("vendor/model-2025");
        response.Usage!.InputTokenCount.Should().Be(11);
        response.Usage.OutputTokenCount.Should().Be(7);
    }

    [Fact]
    public async Task GetResponseAsync_WhenUsageIsMissing_ReportsNoTokens()
    {
        // Arrange
        var body = new { model = "vendor/model", choices = new[] { new { message = new { role = "assistant", content = "Hi" } } } };
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, body);

        // Act
        var response = await AskAsync(handler);

        // Assert
        response.Text.Should().Be("Hi");
        response.Usage?.InputTokenCount.Should().BeNull();
        response.Usage?.OutputTokenCount.Should().BeNull();
    }

    [Fact]
    public async Task GetResponseAsync_WhenTheAnswerIsOkButCarriesAnError_Throws()
    {
        // Arrange
        var body = new { error = new { code = 502, message = "Provider disconnected" } };
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, body);

        // Act
        var act = () => AskAsync(handler);

        // Assert
        var exception = await act.Should().ThrowAsync<OpenRouterIntegrationException>();
        exception.Which.Code.Should().Be("openrouter_integration_failed");
        exception.Which.Message.Should().Contain("could not be read").And.NotContain("Provider disconnected");
    }

    [Fact]
    public async Task GetResponseAsync_WhenAChoiceCarriesAnError_Throws()
    {
        // Arrange
        var body = new { model = "vendor/model", choices = new[] { new { error = new { code = 500, message = "Boom" } } } };
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, body);

        // Act
        var act = () => AskAsync(handler);

        // Assert
        await act.Should().ThrowAsync<OpenRouterIntegrationException>().WithMessage("*could not be read*");
    }

    [Theory]
    [InlineData("{\"choices\": []}")]
    [InlineData("{\"model\": \"vendor/model\"}")]
    public async Task GetResponseAsync_WhenThereAreNoChoices_Throws(string body)
    {
        // Act
        var act = () => AskAsync(JsonBody(body));

        // Assert
        await act.Should().ThrowAsync<OpenRouterIntegrationException>().WithMessage("*could not be read*");
    }

    // A reasoning model that spends its whole token limit on thinking ends up here (finish_reason "length").
    [Theory]
    [InlineData("{\"choices\": [{\"finish_reason\": \"length\", \"message\": {\"role\": \"assistant\", \"content\": null}}]}")]
    [InlineData("{\"choices\": [{\"message\": {\"role\": \"assistant\", \"content\": \"\"}}]}")]
    [InlineData("{\"choices\": [{\"message\": {\"role\": \"assistant\", \"content\": \"   \"}}]}")]
    public async Task GetResponseAsync_WhenThereIsNoText_Throws(string body)
    {
        // Act
        var act = () => AskAsync(JsonBody(body));

        // Assert
        await act.Should().ThrowAsync<OpenRouterIntegrationException>().WithMessage("*empty answer*");
    }

    [Fact]
    public async Task GetResponseAsync_WhenTheBodyIsNotJson_Throws()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Text(HttpStatusCode.OK, "<html>Bad gateway</html>");

        // Act
        var act = () => AskAsync(handler);

        // Assert
        await act.Should().ThrowAsync<OpenRouterIntegrationException>().WithMessage("*could not be read*");
    }

    // --- failures ----------------------------------------------------------------------------

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "rejected the API key")]
    [InlineData(HttpStatusCode.PaymentRequired, "insufficient credits")]
    [InlineData(HttpStatusCode.TooManyRequests, "rate limit")]
    [InlineData(HttpStatusCode.BadGateway, "returned 502")]
    [InlineData(HttpStatusCode.BadRequest, "returned 400")]
    public async Task GetResponseAsync_OnAnErrorStatus_ThrowsAnIntegrationException(HttpStatusCode status, string expectedMessage)
    {
        // Arrange
        var body = new { error = new { code = (int)status, message = "details for the log" } };
        var handler = StubHttpMessageHandler.Json(status, body);

        // Act
        var act = () => AskAsync(handler);

        // Assert
        var exception = await act.Should().ThrowAsync<OpenRouterIntegrationException>();
        exception.Which.Code.Should().Be("openrouter_integration_failed");
        exception.Which.Message.Should().Contain(expectedMessage).And.NotContain("details for the log");
    }

    [Fact]
    public async Task GetResponseAsync_OnAnErrorStatusWithAnHtmlBody_StillThrowsAnIntegrationException()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Text(HttpStatusCode.ServiceUnavailable, "<html>down</html>");

        // Act
        var act = () => AskAsync(handler);

        // Assert
        await act.Should().ThrowAsync<OpenRouterIntegrationException>().WithMessage("*returned 503*");
    }

    // The library retries 429 and 5xx on its own by default; every retry of a model call is billed again
    // and keeps the user waiting, so the request must be sent exactly once.
    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    public async Task GetResponseAsync_OnATransientErrorStatus_DoesNotRetry(HttpStatusCode status)
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(status, new { error = new { code = (int)status, message = "try later" } });

        // Act
        var act = () => AskAsync(handler);

        // Assert
        await act.Should().ThrowAsync<OpenRouterIntegrationException>();
        handler.Requests.Should().ContainSingle();
    }

    [Fact]
    public async Task GetResponseAsync_WhenOpenRouterIsUnreachable_ThrowsWithoutRetrying()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Throwing(new HttpRequestException("connection refused"));

        // Act
        var act = () => AskAsync(handler);

        // Assert
        await act.Should().ThrowAsync<OpenRouterIntegrationException>().WithMessage("*unreachable*connection refused*");
        handler.Requests.Should().ContainSingle();
    }

    [Fact]
    public async Task GetResponseAsync_WhenTheRequestTimesOut_Throws()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Throwing(new TaskCanceledException("timeout"));

        // Act
        var act = () => AskAsync(handler);

        // Assert
        await act.Should().ThrowAsync<OpenRouterIntegrationException>().WithMessage("*did not answer in time*");
    }

    // The library has its own 100 second network timeout; this proves the configured one replaces it. The fact's
    // timeout turns "the configured value was ignored" into a failure instead of a 100 second wait.
    [Fact(Timeout = 15_000)]
    public async Task GetResponseAsync_GivesUpAfterTheConfiguredTimeout()
    {
        // Act
        var act = () => AskAsync(new HangingHandler(), timeoutSeconds: 1);

        // Assert
        await act.Should().ThrowAsync<OpenRouterIntegrationException>().WithMessage("*did not answer in time*");
    }

    [Fact]
    public async Task GetResponseAsync_WhenTheCallerCancels_PropagatesTheCancellation()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Completion());
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        // Act
        var act = () => AskAsync(handler, cancellationToken: cancellation.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // --- configuration -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetResponseAsync_WithoutAnApiKey_ThrowsAndSendsNothing(string apiKey)
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Completion());

        // Act
        var act = () => AskAsync(handler, apiKey: apiKey);

        // Assert
        await act.Should().ThrowAsync<OpenRouterIntegrationException>().WithMessage("*API key is not configured*");
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task GetResponseAsync_WithoutAModel_ThrowsAndSendsNothing()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Completion());

        // Act
        var act = () => AskAsync(handler, model: "");

        // Assert
        await act.Should().ThrowAsync<OpenRouterIntegrationException>().WithMessage("*model is not configured*");
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task AddOpenRouter_LimitsTheHttpClientToTheConfiguredTimeout()
    {
        // Arrange: the default HttpClient timeout is 100 seconds, which would cut a request configured for longer
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOpenRouter(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["integrations:openrouter:TimeoutSeconds"] = "45" })
            .Build());

        // Act
        await using var provider = services.BuildServiceProvider();
        var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient(OpenRouterExtensions.HttpClientName);

        // Assert
        httpClient.Timeout.Should().Be(TimeSpan.FromSeconds(45));
    }

    [Fact]
    public async Task AddOpenRouter_WithoutAKey_DoesNotBreakTheContainerUntilTheClientIsUsed()
    {
        // Arrange: a fresh checkout has no key, and the rest of the application must still start and resolve services
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOpenRouter(new ConfigurationBuilder().Build());

        // Act
        await using var provider = services.BuildServiceProvider(validateScopes: true);
        await using var scope = provider.CreateAsyncScope();
        var resolve = () => scope.ServiceProvider.GetRequiredKeyedService<IChatClient>(OpenRouterExtensions.ServiceKey);

        // Assert
        resolve.Should().Throw<OpenRouterIntegrationException>().WithMessage("*API key is not configured*");
    }
}
