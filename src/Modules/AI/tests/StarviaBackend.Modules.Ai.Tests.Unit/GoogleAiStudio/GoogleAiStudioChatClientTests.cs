using System.Net;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Ai.Infrastructure.GoogleAiStudio;
using StarviaBackend.Modules.Ai.Tests.Unit.Fakes;

namespace StarviaBackend.Modules.Ai.Tests.Unit.GoogleAiStudio;

/// <summary>
/// The Google AI Studio integration is the official Google Gen AI library behind <see cref="IChatClient"/>, so
/// these tests go through the real registration and a fake HTTP handler: they pin what goes on the wire, and how
/// the library and our error translation behave with the answers Google can give.
/// </summary>
public sealed class GoogleAiStudioChatClientTests
{
    private const string ApiKey = "AIza-test-key";
    private const string Model = "gemini-test-model";
    private const string ApiKeyHeader = "x-goog-api-key";

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
        string model = Model,
        int timeoutSeconds = 120,
        CancellationToken cancellationToken = default)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["integrations:googleaistudio:ApiKey"] = apiKey,
                ["integrations:googleaistudio:Model"] = model,
                ["integrations:googleaistudio:TimeoutSeconds"] = timeoutSeconds.ToString(),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddGoogleAiStudio(configuration);

        // Swaps the network for the fake; the rest of the client's pipeline stays as registered. The registration
        // also limits the HttpClient to the configured timeout; lifting that here lets the tests see the timeout
        // of the Google library itself, which is the one that would otherwise be hidden behind the HttpClient's.
        services.AddHttpClient(GoogleAiStudioExtensions.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .ConfigureHttpClient(client => client.Timeout = Timeout.InfiniteTimeSpan);

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var client = scope.ServiceProvider.GetRequiredService<IChatClient>();
        return await client.GetResponseAsync(messages ?? Conversation, options, cancellationToken);
    }

    private static object Generated(
        string text = "Hello!",
        string? modelVersion = "gemini-3.5-flash-lite-001",
        object? usage = null) => new
    {
        candidates = new[]
        {
            new { content = new { role = "model", parts = new[] { new { text } } }, finishReason = "STOP" },
        },
        usageMetadata = usage ?? new { promptTokenCount = 11, candidatesTokenCount = 7, totalTokenCount = 18 },
        modelVersion,
    };

    /// <summary>The body of a Google API error, with the reason Google puts in its details when it has one.</summary>
    private static object GoogleError(int code, string status, string message, string? reason = null)
    {
        var error = new Dictionary<string, object?> { ["code"] = code, ["message"] = message, ["status"] = status };

        if (reason is not null)
        {
            error["details"] = new[]
            {
                new Dictionary<string, string>
                {
                    ["@type"] = "type.googleapis.com/google.rpc.ErrorInfo",
                    ["reason"] = reason,
                },
            };
        }

        return new { error };
    }

    private static StubHttpMessageHandler JsonBody(string body) =>
        new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json"),
        });

    /// <summary>Never answers; only the caller's token (or a timeout wired into it) can end the request.</summary>
    private sealed class HangingHandler : HttpMessageHandler
    {
        private readonly TaskCompletionSource _started = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>Completes once the request has reached the network, so a test can cancel a running one.</summary>
        public Task Started => _started.Task;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _started.TrySetResult();
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new InvalidOperationException("Unreachable: the delay only ends by cancellation.");
        }
    }

    // --- request -----------------------------------------------------------------------------

    [Fact]
    public async Task GetResponseAsync_PostsTheConversationToTheGeminiApiWithTheKeyInTheHeader()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Generated());

        // Act
        await AskAsync(handler);

        // Assert
        var sent = handler.Requests.Should().ContainSingle().Subject;
        sent.Method.Should().Be(HttpMethod.Post);
        sent.Uri.Should().Be(new Uri($"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent"));
        sent.Header(ApiKeyHeader).Should().Be(ApiKey);
        sent.Uri.ToString().Should().NotContain(ApiKey, "a key in the address ends up in logs");
        sent.Authorization.Should().BeNull();

        // Google names the assistant "model", and takes the instructions apart from the conversation.
        var contents = sent.Json.GetProperty("contents").EnumerateArray()
            .Select(c => (c.GetProperty("role").GetString(), c.GetProperty("parts")[0].GetProperty("text").GetString()))
            .ToList();
        contents.Should().Equal(
            ("user", "Write a post."),
            ("model", "Here it is."),
            ("user", "Shorter."));
        sent.Json.GetProperty("systemInstruction").GetProperty("parts")[0].GetProperty("text").GetString()
            .Should().Be("Be brief.");
    }

    [Fact]
    public async Task GetResponseAsync_UsesTheConfiguredModel()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Generated());

        // Act
        await AskAsync(handler, model: "gemini-configured");

        // Assert
        handler.Requests.Single().Uri.AbsolutePath.Should().Be("/v1beta/models/gemini-configured:generateContent");
    }

    [Fact]
    public async Task GetResponseAsync_PrefersTheModelOfTheOptions()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Generated());

        // Act
        await AskAsync(handler, new ChatOptions { ModelId = "gemini-other" });

        // Assert
        handler.Requests.Single().Uri.AbsolutePath.Should().Be("/v1beta/models/gemini-other:generateContent");
    }

    [Fact]
    public async Task GetResponseAsync_SendsOnlyTheOptionsThatWereAskedFor()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Generated());

        // Act
        await AskAsync(handler);

        // Assert: Google tells Gemini 3 users to leave the sampling parameters at their defaults
        var json = handler.Requests.Single().Json;
        if (json.TryGetProperty("generationConfig", out var configuration))
        {
            foreach (var name in new[] { "temperature", "topP", "topK", "maxOutputTokens", "responseMimeType", "thinkingConfig" })
            {
                configuration.TryGetProperty(name, out _).Should().BeFalse($"{name} was not asked for");
            }
        }
    }

    [Fact]
    public async Task GetResponseAsync_SendsTheRequestedJsonModeAndTokenLimit()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Generated());
        var options = new ChatOptions { ResponseFormat = ChatResponseFormat.Json, MaxOutputTokens = 300 };

        // Act
        await AskAsync(handler, options);

        // Assert
        var configuration = handler.Requests.Single().Json.GetProperty("generationConfig");
        configuration.GetProperty("responseMimeType").GetString().Should().Be("application/json");
        configuration.GetProperty("maxOutputTokens").GetInt32().Should().Be(300);
    }

    [Fact]
    public async Task GetResponseAsync_KeepsNonAsciiTextIntact()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Generated());
        var messages = new[] { new ChatMessage(ChatRole.User, "Zażółć gęślą jaźń ☕") };

        // Act
        await AskAsync(handler, messages: messages);

        // Assert
        handler.Requests.Single().Json.GetProperty("contents")[0].GetProperty("parts")[0].GetProperty("text").GetString()
            .Should().Be("Zażółć gęślą jaźń ☕");
    }

    [Fact]
    public async Task GetResponseAsync_ReachesGoogleAiStudioEvenWhenTheEnvironmentSelectsVertexAi()
    {
        // The library switches to Vertex AI (another host, another kind of credentials) when these variables are
        // set on the machine. The registration decides for AI Studio, so they must have no say.
        var variables = new Dictionary<string, string?>
        {
            ["GOOGLE_GENAI_USE_ENTERPRISE"] = "true",
            ["GOOGLE_GENAI_USE_VERTEXAI"] = "true",
            ["GOOGLE_CLOUD_PROJECT"] = "some-project",
            ["GOOGLE_CLOUD_LOCATION"] = "us-central1",
        };
        var previous = variables.Keys.ToDictionary(name => name, Environment.GetEnvironmentVariable);
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Generated());

        try
        {
            foreach (var (name, value) in variables)
            {
                Environment.SetEnvironmentVariable(name, value);
            }

            // Act
            await AskAsync(handler);
        }
        finally
        {
            foreach (var (name, value) in previous)
            {
                Environment.SetEnvironmentVariable(name, value);
            }
        }

        // Assert
        var sent = handler.Requests.Should().ContainSingle().Subject;
        sent.Uri.Host.Should().Be("generativelanguage.googleapis.com");
        sent.Header(ApiKeyHeader).Should().Be(ApiKey);
    }

    // --- answer ------------------------------------------------------------------------------

    [Fact]
    public async Task GetResponseAsync_ReturnsTheAnswerTheModelAndTheTokenUsage()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Generated("Fresh coffee!"));

        // Act
        var response = await AskAsync(handler);

        // Assert
        response.Text.Should().Be("Fresh coffee!");
        response.ModelId.Should().Be("gemini-3.5-flash-lite-001");
        response.Usage!.InputTokenCount.Should().Be(11);
        response.Usage.OutputTokenCount.Should().Be(7);
    }

    // Gemini reports the thinking apart from the answer but bills both as output, so the count has to add them up.
    [Fact]
    public async Task GetResponseAsync_CountsThinkingTokensAsOutput()
    {
        // Arrange
        var usage = new { promptTokenCount = 11, candidatesTokenCount = 22, thoughtsTokenCount = 5, totalTokenCount = 38 };
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Generated(usage: usage));

        // Act
        var response = await AskAsync(handler);

        // Assert
        response.Usage!.InputTokenCount.Should().Be(11);
        response.Usage.OutputTokenCount.Should().Be(27, "22 of the answer and 5 of thinking");
        response.Usage.ReasoningTokenCount.Should().Be(5);
    }

    [Fact]
    public async Task GetResponseAsync_WhenUsageIsMissing_ReportsNoTokens()
    {
        // Arrange
        var body = new
        {
            modelVersion = "gemini-3.5-flash-lite-001",
            candidates = new[] { new { content = new { role = "model", parts = new[] { new { text = "Hi" } } } } },
        };
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, body);

        // Act
        var response = await AskAsync(handler);

        // Assert
        response.Text.Should().Be("Hi");
        response.Usage?.InputTokenCount.Should().BeNull();
        response.Usage?.OutputTokenCount.Should().BeNull();
    }

    [Fact]
    public async Task GetResponseAsync_LeavesTheThoughtsOutOfTheAnswer()
    {
        // Arrange
        var body = new
        {
            candidates = new[]
            {
                new
                {
                    content = new
                    {
                        role = "model",
                        parts = new object[]
                        {
                            new { text = "Let me think about the post.", thought = true },
                            new { text = "{\"title\":\"T\",\"body\":\"B\"}" },
                        },
                    },
                    finishReason = "STOP",
                },
            },
        };
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, body);

        // Act
        var response = await AskAsync(handler);

        // Assert
        response.Text.Should().Be("{\"title\":\"T\",\"body\":\"B\"}");
    }

    [Fact]
    public async Task GetResponseAsync_WhenTheAnswerNamesNoModel_ReportsTheConfiguredOne()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Generated(modelVersion: null));

        // Act
        var response = await AskAsync(handler, model: "gemini-configured");

        // Assert
        response.ModelId.Should().Be("gemini-configured");
    }

    // Google answers 200 and no text when it stops the generation, and says why in the response.
    [Theory]
    [InlineData("{}", "returned an empty answer")]
    [InlineData("{\"candidates\": []}", "returned an empty answer")]
    [InlineData("{\"candidates\": [{\"content\": {\"role\": \"model\", \"parts\": [{\"text\": \"   \"}]}, \"finishReason\": \"STOP\"}]}", "returned an empty answer")]
    [InlineData("{\"error\": {\"code\": 500, \"message\": \"Internal\", \"status\": \"INTERNAL\"}}", "returned an empty answer")]
    [InlineData("{\"candidates\": [{\"content\": {\"role\": \"model\"}, \"finishReason\": \"MAX_TOKENS\"}]}", "ran out of tokens")]
    [InlineData("{\"promptFeedback\": {\"blockReason\": \"PROHIBITED_CONTENT\"}}", "refused to answer (PROHIBITED_CONTENT)")]
    [InlineData("{\"candidates\": [{\"finishReason\": \"SAFETY\"}]}", "refused to answer (SAFETY)")]
    [InlineData("{\"candidates\": [{\"content\": {\"role\": \"model\"}, \"finishReason\": \"RECITATION\"}]}", "refused to answer (RECITATION)")]
    public async Task GetResponseAsync_WhenThereIsNoText_ThrowsAndSaysWhy(string body, string expectedMessage)
    {
        // Act
        var act = () => AskAsync(JsonBody(body));

        // Assert
        var exception = await act.Should().ThrowAsync<GoogleAiStudioIntegrationException>();
        exception.Which.Code.Should().Be("google_ai_studio_integration_failed");
        exception.Which.Message.Should().Contain(expectedMessage);
    }

    // Each of these shapes makes the library throw something other than its own API exception.
    [Theory]
    [InlineData("<html>Bad gateway</html>")]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("{\"candidates\": \"x\"}")]
    [InlineData("{\"candidates\": [{\"content\": \"x\"}]}")]
    [InlineData("{\"candidates\": [{\"content\": {\"parts\": [null]}}]}")]
    [InlineData("{\"candidates\": [{\"content\": {\"parts\": [{\"text\": \"cut off")]
    public async Task GetResponseAsync_WhenTheBodyIsNotAGeneratedContent_Throws(string body)
    {
        // Act
        var act = () => AskAsync(JsonBody(body));

        // Assert
        await act.Should().ThrowAsync<GoogleAiStudioIntegrationException>().WithMessage("*could not be read*");
    }

    // --- failures ----------------------------------------------------------------------------

    [Theory]
    [InlineData(400, "INVALID_ARGUMENT", "API key not valid. Please pass a valid API key.", null, "rejected the API key")]
    [InlineData(400, "INVALID_ARGUMENT", "details for the log", "API_KEY_INVALID", "rejected the API key")]
    [InlineData(401, "UNAUTHENTICATED", "details for the log", null, "rejected the API key")]
    [InlineData(400, "INVALID_ARGUMENT", "details for the log", null, "returned 400 (INVALID_ARGUMENT)")]
    [InlineData(400, "FAILED_PRECONDITION", "details for the log", null, "returned 400 (FAILED_PRECONDITION)")]
    [InlineData(403, "PERMISSION_DENIED", "details for the log", null, "denied access")]
    [InlineData(404, "NOT_FOUND", "details for the log", null, "does not know the configured model")]
    [InlineData(429, "RESOURCE_EXHAUSTED", "details for the log", null, "quota or rate limit")]
    [InlineData(500, "INTERNAL", "details for the log", null, "returned 500 (INTERNAL)")]
    [InlineData(503, "UNAVAILABLE", "details for the log", null, "returned 503 (UNAVAILABLE)")]
    [InlineData(504, "DEADLINE_EXCEEDED", "details for the log", null, "returned 504 (DEADLINE_EXCEEDED)")]
    public async Task GetResponseAsync_OnAnErrorStatus_ThrowsAnIntegrationException(
        int statusCode,
        string googleStatus,
        string googleMessage,
        string? reason,
        string expectedMessage)
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json((HttpStatusCode)statusCode, GoogleError(statusCode, googleStatus, googleMessage, reason));

        // Act
        var act = () => AskAsync(handler);

        // Assert
        var exception = await act.Should().ThrowAsync<GoogleAiStudioIntegrationException>();
        exception.Which.Code.Should().Be("google_ai_studio_integration_failed");
        exception.Which.Message.Should().Contain(expectedMessage).And.NotContain(googleMessage);
    }

    [Fact]
    public async Task GetResponseAsync_OnAnErrorStatusWithAnHtmlBody_StillThrowsAnIntegrationException()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Text(HttpStatusCode.BadGateway, "<html>down</html>");

        // Act
        var act = () => AskAsync(handler);

        // Assert
        await act.Should().ThrowAsync<GoogleAiStudioIntegrationException>().WithMessage("*returned 502*");
    }

    // A retried model call is billed again, counts against the quota and keeps the user waiting,
    // so the request must be sent exactly once.
    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    public async Task GetResponseAsync_OnATransientErrorStatus_DoesNotRetry(HttpStatusCode status)
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(status, GoogleError((int)status, "UNAVAILABLE", "try later"));

        // Act
        var act = () => AskAsync(handler);

        // Assert
        await act.Should().ThrowAsync<GoogleAiStudioIntegrationException>();
        handler.Requests.Should().ContainSingle();
    }

    [Fact]
    public async Task GetResponseAsync_WhenGoogleIsUnreachable_ThrowsWithoutRetrying()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Throwing(new HttpRequestException("connection refused"));

        // Act
        var act = () => AskAsync(handler);

        // Assert
        await act.Should().ThrowAsync<GoogleAiStudioIntegrationException>().WithMessage("*unreachable*connection refused*");
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
        await act.Should().ThrowAsync<GoogleAiStudioIntegrationException>().WithMessage("*did not answer in time*");
    }

    // The registration gives the library the configured timeout; this proves it is applied (the HttpClient's own
    // is lifted by the helper). The fact's timeout turns "the configured value was ignored" into a failure
    // instead of a hang.
    [Fact(Timeout = 15_000)]
    public async Task GetResponseAsync_GivesUpAfterTheConfiguredTimeout()
    {
        // Act
        var act = () => AskAsync(new HangingHandler(), timeoutSeconds: 1);

        // Assert
        await act.Should().ThrowAsync<GoogleAiStudioIntegrationException>().WithMessage("*did not answer in time*");
    }

    // A user who leaves must stop the model call, and that is not a failure of Google to report.
    [Fact(Timeout = 15_000)]
    public async Task GetResponseAsync_WhenTheCallerCancelsTheRunningRequest_PropagatesTheCancellation()
    {
        // Arrange
        var handler = new HangingHandler();
        using var cancellation = new CancellationTokenSource();
        var request = AskAsync(handler, cancellationToken: cancellation.Token);
        await handler.Started;

        // Act
        await cancellation.CancelAsync();
        var act = () => request;

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
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Generated());

        // Act
        var act = () => AskAsync(handler, apiKey: apiKey);

        // Assert
        await act.Should().ThrowAsync<GoogleAiStudioIntegrationException>().WithMessage("*API key is not configured*");
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task GetResponseAsync_WithoutAModel_ThrowsAndSendsNothing()
    {
        // Arrange
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Generated());

        // Act
        var act = () => AskAsync(handler, model: "");

        // Assert
        await act.Should().ThrowAsync<GoogleAiStudioIntegrationException>().WithMessage("*model is not configured*");
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task AddGoogleAiStudio_LimitsTheHttpClientToTheConfiguredTimeout()
    {
        // Arrange: the default HttpClient timeout is 100 seconds, which would cut a request configured for longer
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddGoogleAiStudio(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["integrations:googleaistudio:TimeoutSeconds"] = "45" })
            .Build());

        // Act
        await using var provider = services.BuildServiceProvider();
        var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient(GoogleAiStudioExtensions.HttpClientName);

        // Assert
        httpClient.Timeout.Should().Be(TimeSpan.FromSeconds(45));
    }

    [Fact]
    public async Task AddGoogleAiStudio_WithoutAKey_DoesNotBreakTheContainerUntilTheClientIsUsed()
    {
        // Arrange: a fresh checkout has no key, and the rest of the application must still start and resolve services
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddGoogleAiStudio(new ConfigurationBuilder().Build());

        // Act
        await using var provider = services.BuildServiceProvider(validateScopes: true);
        await using var scope = provider.CreateAsyncScope();
        var resolve = () => scope.ServiceProvider.GetRequiredService<IChatClient>();

        // Assert
        resolve.Should().Throw<GoogleAiStudioIntegrationException>().WithMessage("*API key is not configured*");
    }

    [Fact]
    public async Task AddGoogleAiStudio_WhenOnlyTheKeyIsConfigured_AsksGemini35FlashLite()
    {
        // Arrange: only the key is configured, as on a fresh checkout that was given one
        var handler = StubHttpMessageHandler.Json(HttpStatusCode.OK, Generated());
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddGoogleAiStudio(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["integrations:googleaistudio:ApiKey"] = ApiKey })
            .Build());
        services.AddHttpClient(GoogleAiStudioExtensions.HttpClientName).ConfigurePrimaryHttpMessageHandler(() => handler);

        // Act
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IChatClient>().GetResponseAsync(Conversation);

        // Assert
        handler.Requests.Single().Uri.AbsolutePath.Should().Be("/v1beta/models/gemini-3.5-flash-lite:generateContent");
    }
}
