using System.ClientModel;
using System.ClientModel.Primitives;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;

namespace StarviaBackend.Modules.Ai.Infrastructure.OpenRouter;

internal static class OpenRouterExtensions
{
    public const string HttpClientName = "openrouter";

    /// <summary>
    /// The key of the OpenRouter <see cref="IChatClient"/>. It is registered by key and not as the default one on
    /// purpose: every text of the module is written by Google AI Studio (the unkeyed <see cref="IChatClient"/>),
    /// and OpenRouter is kept for the images, which have to ask for it explicitly.
    /// </summary>
    public const string ServiceKey = "openrouter";

    /// <summary>
    /// Registers an <see cref="IChatClient"/> backed by OpenRouter under <see cref="ServiceKey"/>. There is no
    /// client code of our own: OpenRouter speaks the OpenAI protocol, so the official OpenAI library is pointed at
    /// its endpoint and exposed through the Microsoft.Extensions.AI abstraction.
    /// </summary>
    public static IServiceCollection AddOpenRouter(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OpenRouterOptions>().Bind(configuration.GetSection(OpenRouterOptions.SectionName));

        services.AddHttpClient(HttpClientName, (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<OpenRouterOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        // Scoped, not singleton: an HttpClient taken from the factory must not outlive its handler, or DNS changes
        // are never picked up. Building the client is cheap next to a model call.
        services.AddKeyedScoped<IChatClient>(ServiceKey, (serviceProvider, _) => CreateChatClient(serviceProvider));

        return services;
    }

    private static IChatClient CreateChatClient(IServiceProvider services)
    {
        var settings = services.GetRequiredService<IOptions<OpenRouterOptions>>().Value;

        // Checked here and not at startup: the key is a secret that is absent on a fresh checkout, and only the
        // AI endpoints need it. A missing key is reported to the caller instead of failing the whole application.
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new OpenRouterIntegrationException("OpenRouter API key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.Model))
        {
            throw new OpenRouterIntegrationException("OpenRouter model is not configured.");
        }

        var clientOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(settings.BaseUrl),

            // Requests go through the factory's HttpClient, so handlers, DNS refresh and test doubles all work.
            Transport = new HttpClientPipelineTransport(services.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)),

            // The library caps a request at 100 seconds by default, which would silently override a longer timeout.
            NetworkTimeout = TimeSpan.FromSeconds(settings.TimeoutSeconds),

            // No hidden retries: a user is waiting, a retried call is billed again, and the caller can simply retry.
            RetryPolicy = new ClientRetryPolicy(maxRetries: 0),
        };

        var chatClient = new OpenAIClient(new ApiKeyCredential(settings.ApiKey), clientOptions)
            .GetChatClient(settings.Model)
            .AsIChatClient();

        return new ErrorTranslatingChatClient(chatClient, services.GetRequiredService<ILogger<ErrorTranslatingChatClient>>());
    }
}
