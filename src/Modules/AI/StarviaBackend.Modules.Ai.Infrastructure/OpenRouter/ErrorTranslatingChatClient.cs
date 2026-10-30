using System.ClientModel;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace StarviaBackend.Modules.Ai.Infrastructure.OpenRouter;

/// <summary>
/// Middleware around the OpenAI-backed <see cref="IChatClient"/>: every failure of the library or of OpenRouter
/// becomes an <see cref="OpenRouterIntegrationException"/>, so the caller gets a clean HTTP 400 instead of an
/// unhandled 500. It only covers <see cref="GetResponseAsync"/>, the one call the module makes.
/// </summary>
internal sealed class ErrorTranslatingChatClient(IChatClient innerClient, ILogger<ErrorTranslatingChatClient> logger)
    : DelegatingChatClient(innerClient)
{
    private const int MaxLoggedErrorLength = 500;

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ChatResponse response;

        try
        {
            response = await base.GetResponseAsync(messages, options, cancellationToken);
        }
        catch (ClientResultException ex)
        {
            throw ToFailure(ex);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // The caller did not cancel, so this is the request timing out.
            logger.LogWarning(ex, "OpenRouter did not answer in time");
            throw new OpenRouterIntegrationException("OpenRouter did not answer in time.");
        }
        catch (Exception ex) when (ex is JsonException or ArgumentException or InvalidOperationException or NullReferenceException)
        {
            // The library parses the body lazily and its accessors are not guarded, so a 2xx answer that is not
            // JSON, or has no choices (OpenRouter can answer 200 with only an `error` object), escapes as one of
            // these instead of a ClientResultException. Only the library call is inside this try, and the unit
            // tests pin each of those shapes, so an upgrade that changes the exception type fails loudly.
            logger.LogWarning(ex, "OpenRouter returned an answer that is not a chat completion");
            throw new OpenRouterIntegrationException("The OpenRouter answer could not be read.");
        }

        // A reasoning model that spends its whole token limit on thinking answers with no text at all.
        if (string.IsNullOrWhiteSpace(response.Text))
        {
            throw new OpenRouterIntegrationException("OpenRouter returned an empty answer.");
        }

        logger.LogInformation(
            "OpenRouter completion by {Model}: {InputTokens} input / {OutputTokens} output tokens",
            response.ModelId,
            response.Usage?.InputTokenCount,
            response.Usage?.OutputTokenCount);

        return response;
    }

    /// <summary>
    /// The exception shown to callers only carries the status, so the provider's own explanation
    /// (unknown model, moderation, no matching provider, ...) is kept in the log for whoever has to fix it.
    /// </summary>
    private OpenRouterIntegrationException ToFailure(ClientResultException exception)
    {
        // Status 0: the request never produced a response (connection refused, DNS, TLS, ...).
        if (exception.Status == 0)
        {
            logger.LogWarning(exception, "OpenRouter is unreachable");
            return new OpenRouterIntegrationException($"OpenRouter unreachable: {(exception.InnerException ?? exception).Message}");
        }

        logger.LogWarning(
            "OpenRouter returned {StatusCode}: {Body}",
            exception.Status,
            Truncate(exception.GetRawResponse()?.Content.ToString()));

        return exception.Status switch
        {
            401 => new OpenRouterIntegrationException("OpenRouter rejected the API key."),
            402 => new OpenRouterIntegrationException("The OpenRouter account has insufficient credits."),
            429 => new OpenRouterIntegrationException("OpenRouter rate limit reached. Try again in a moment."),
            _ => new OpenRouterIntegrationException($"OpenRouter returned {exception.Status}."),
        };
    }

    private static string? Truncate(string? value) =>
        value is { Length: > MaxLoggedErrorLength } ? value[..MaxLoggedErrorLength] : value;
}
