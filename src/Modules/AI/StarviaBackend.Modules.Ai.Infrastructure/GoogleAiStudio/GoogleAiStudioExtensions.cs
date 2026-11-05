using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace StarviaBackend.Modules.Ai.Infrastructure.GoogleAiStudio;

internal static class GoogleAiStudioExtensions
{
    public const string HttpClientName = "googleaistudio";

    /// <summary>
    /// Registers the <see cref="IChatClient"/> that writes every text of the module: the official Google Gen AI
    /// </summary>
    public static IServiceCollection AddGoogleAiStudio(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<GoogleAiStudioOptions>().Bind(configuration.GetSection(GoogleAiStudioOptions.SectionName));

        services.AddHttpClient(HttpClientName, (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<GoogleAiStudioOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        services.AddScoped<IChatClient>(CreateChatClient);

        return services;
    }

    private static IChatClient CreateChatClient(IServiceProvider services)
    {
        var settings = services.GetRequiredService<IOptions<GoogleAiStudioOptions>>().Value;

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new GoogleAiStudioIntegrationException("Google AI Studio API key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.Model))
        {
            throw new GoogleAiStudioIntegrationException("Google AI Studio model is not configured.");
        }

        var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();

        var client = new Client(
            // The library switches to Vertex AI when GOOGLE_GENAI_USE_ENTERPRISE (or GOOGLE_GENAI_USE_VERTEXAI)
            // is set in the environment, and it would then ignore this key. Texts must come from AI Studio, so the
            // choice is made here and no environment of the machine can change it.
            enterprise: false,
            apiKey: settings.ApiKey,
            httpOptions: new HttpOptions
            {
                // The library's own limit of a request, in milliseconds. The HttpClient of the registration is
                // limited to the same value, so neither of them cuts a long answer short (HttpClient: 100 seconds).
                Timeout = (int)TimeSpan.FromSeconds(settings.TimeoutSeconds).TotalMilliseconds,

                // No hidden retries: a user is waiting, a retried call is billed again (and counts against the
                // quota), and the caller can simply retry. The library does not retry unless it is told to; this
                // says so in case that default ever changes, and the tests send a failing request to prove it.
                RetryOptions = new HttpRetryOptions { Attempts = 1 },
            },
            // Requests go through the factory's HttpClient, so handlers, DNS refresh and test doubles all work.
            clientOptions: new ClientOptions { HttpClientFactory = () => httpClientFactory.CreateClient(HttpClientName) });

        return new GoogleAiStudioChatClient(
            client.AsIChatClient(settings.Model),
            client,
            services.GetRequiredService<ILogger<GoogleAiStudioChatClient>>());
    }
}
