using System.Net;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Ai.Infrastructure.GoogleAiStudio;
using StarviaBackend.Modules.Ai.Infrastructure.OpenRouter;
using StarviaBackend.Modules.Ai.Tests.Unit.Fakes;

namespace StarviaBackend.Modules.Ai.Tests.Unit.GoogleAiStudio;

/// <summary>
/// Every text of the module must be written by Google AI Studio, and OpenRouter is kept for the images. The use
/// cases ask for a plain <see cref="IChatClient"/>, so these tests register both providers the way the module
/// does and check which one that request reaches, whatever the other one's configuration looks like.
/// </summary>
public sealed class ChatClientRoutingTests
{
    private const string GoogleKey = "AIza-test-key";
    private const string OpenRouterKey = "sk-or-test";

    private static readonly ChatMessage[] Question = [new(ChatRole.User, "Write a post.")];

    private static readonly object GeneratedByGoogle = new
    {
        candidates = new[] { new { content = new { role = "model", parts = new[] { new { text = "from google" } } } } },
    };

    private static readonly object CompletedByOpenRouter = new
    {
        choices = new[] { new { message = new { role = "assistant", content = "from openrouter" } } },
    };

    private sealed class Providers
    {
        public StubHttpMessageHandler Google { get; } = StubHttpMessageHandler.Json(HttpStatusCode.OK, GeneratedByGoogle);
        public StubHttpMessageHandler OpenRouter { get; } = StubHttpMessageHandler.Json(HttpStatusCode.OK, CompletedByOpenRouter);
        public ServiceProvider Services { get; }

        public Providers(string googleKey = GoogleKey, string openRouterKey = OpenRouterKey)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["integrations:googleaistudio:ApiKey"] = googleKey,
                    ["integrations:openrouter:ApiKey"] = openRouterKey,
                })
                .Build();

            var services = new ServiceCollection();
            services.AddLogging();

            // The same two calls, in the same order, as the module's own registration.
            services.AddGoogleAiStudio(configuration);
            services.AddOpenRouter(configuration);

            services.AddHttpClient(GoogleAiStudioExtensions.HttpClientName).ConfigurePrimaryHttpMessageHandler(() => Google);
            services.AddHttpClient(OpenRouterExtensions.HttpClientName).ConfigurePrimaryHttpMessageHandler(() => OpenRouter);

            Services = services.BuildServiceProvider(validateScopes: true);
        }
    }

    [Fact]
    public async Task TheDefaultChatClient_WritesWithGoogleAiStudio_AndNeverReachesOpenRouter()
    {
        // Arrange
        var providers = new Providers();
        await using var scope = providers.Services.CreateAsyncScope();

        // Act
        var response = await scope.ServiceProvider.GetRequiredService<IChatClient>().GetResponseAsync(Question);

        // Assert
        response.Text.Should().Be("from google");
        providers.Google.Requests.Should().ContainSingle()
            .Which.Uri.Host.Should().Be("generativelanguage.googleapis.com");
        providers.OpenRouter.Requests.Should().BeEmpty("OpenRouter is not for texts");
    }

    [Fact]
    public async Task TheDefaultChatClient_WithoutAGoogleKey_DoesNotFallBackToOpenRouter()
    {
        // Arrange: OpenRouter is fully configured, Google is not
        var providers = new Providers(googleKey: "");
        await using var scope = providers.Services.CreateAsyncScope();

        // Act
        var act = () => scope.ServiceProvider.GetRequiredService<IChatClient>().GetResponseAsync(Question);

        // Assert
        await act.Should().ThrowAsync<GoogleAiStudioIntegrationException>().WithMessage("*Google AI Studio API key is not configured*");
        providers.OpenRouter.Requests.Should().BeEmpty();
        providers.Google.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task TheDefaultChatClient_DoesNotNeedAnOpenRouterKey()
    {
        // Arrange: OpenRouter is not set up at all, as before the images exist
        var providers = new Providers(openRouterKey: "");
        await using var scope = providers.Services.CreateAsyncScope();

        // Act
        var response = await scope.ServiceProvider.GetRequiredService<IChatClient>().GetResponseAsync(Question);

        // Assert
        response.Text.Should().Be("from google");
    }

    [Fact]
    public async Task TheOpenRouterChatClient_IsOnlyAvailableByItsKey_AndReachesOpenRouter()
    {
        // Arrange
        var providers = new Providers();
        await using var scope = providers.Services.CreateAsyncScope();

        // Act
        var response = await scope.ServiceProvider
            .GetRequiredKeyedService<IChatClient>(OpenRouterExtensions.ServiceKey)
            .GetResponseAsync(Question);

        // Assert
        response.Text.Should().Be("from openrouter");
        providers.OpenRouter.Requests.Should().ContainSingle()
            .Which.Uri.Host.Should().Be("openrouter.ai");
        providers.Google.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task ThePlainChatClients_AreGoogleAlone_SoNothingCanPickOpenRouterByAccident()
    {
        // Arrange
        var providers = new Providers();
        await using var scope = providers.Services.CreateAsyncScope();

        // Act
        var all = scope.ServiceProvider.GetServices<IChatClient>().ToList();

        // Assert: asking for all plain chat clients yields Google's alone, so nothing can pick OpenRouter by accident
        all.Should().ContainSingle().Which.Should().BeOfType<GoogleAiStudioChatClient>();
    }
}
