using System.Text.Json;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace StarviaBackend.Modules.Ai.Infrastructure.GoogleAiStudio;

/// <summary>
/// Middleware around the Google-backed <see cref="IChatClient"/>: every failure of the library or of Google AI
/// Studio becomes a <see cref="GoogleAiStudioIntegrationException"/>, so the caller gets a clean HTTP 400 instead
/// of an unhandled 500. It only covers <see cref="GetResponseAsync"/>, the one call the module makes. It also owns
/// the library's <see cref="Client"/>, which is released together with it at the end of the request.
/// </summary>
internal sealed class GoogleAiStudioChatClient(
    IChatClient innerClient,
    Client sdkClient,
    ILogger<GoogleAiStudioChatClient> logger)
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
        catch (ApiException ex)
        {
            // Before HttpRequestException: it is the base class of ApiException.
            throw ToFailure(ex);
        }
        catch (HttpRequestException ex)
        {
            // The request never produced a response (connection refused, DNS, TLS, ...).
            logger.LogWarning(ex, "Google AI Studio is unreachable");
            throw new GoogleAiStudioIntegrationException($"Google AI Studio unreachable: {(ex.InnerException ?? ex).Message}");
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // The caller did not cancel, so this is the request timing out.
            logger.LogWarning(ex, "Google AI Studio did not answer in time");
            throw new GoogleAiStudioIntegrationException("Google AI Studio did not answer in time.");
        }
        catch (Exception ex) when (ex is JsonException or InvalidCastException or NotSupportedException or NullReferenceException)
        {
            // The library parses the body into loosely typed nodes whose accessors are not guarded, so a 2xx
            // answer that is not the JSON it expects escapes as one of these instead of an ApiException. Only the
            // library call is inside this try, and the unit tests pin each of those shapes, so an upgrade that
            // changes the exception type fails loudly.
            logger.LogWarning(ex, "Google AI Studio returned an answer that is not a generated content response");
            throw new GoogleAiStudioIntegrationException("The Google AI Studio answer could not be read.");
        }

        if (string.IsNullOrWhiteSpace(response.Text))
        {
            throw EmptyAnswer(response);
        }

        // Gemini counts its thinking tokens apart from the answer's, but bills them as output. The convention of
        // Microsoft.Extensions.AI (set by the OpenAI-style providers) is that the output count includes them, and
        // the stored numbers are meant to show what a call cost, so the two are added up here.
        if (response.Usage is { ReasoningTokenCount: { } thinkingTokens } usage)
        {
            usage.OutputTokenCount = (usage.OutputTokenCount ?? 0) + thinkingTokens;
        }

        logger.LogInformation(
            "Google AI Studio completion by {Model}: {InputTokens} input / {OutputTokens} output tokens",
            response.ModelId,
            response.Usage?.InputTokenCount,
            response.Usage?.OutputTokenCount);

        return response;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            sdkClient.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Google answers 200 and no text when it stops the generation: the prompt was blocked, the answer was cut off
    /// by the token limit (a model that thinks can spend all of it on thinking) or it was filtered. The reason is in
    /// the response, and it is the part of the explanation that is safe to show.
    /// </summary>
    private GoogleAiStudioIntegrationException EmptyAnswer(ChatResponse response)
    {
        var raw = response.RawRepresentation as GenerateContentResponse;
        var blockReason = raw?.PromptFeedback?.BlockReason?.ToString();
        var finishReason = raw?.Candidates?.FirstOrDefault()?.FinishReason?.ToString();

        logger.LogWarning(
            "Google AI Studio answered without text: block reason {BlockReason}, finish reason {FinishReason}, {InputTokens} input / {OutputTokens} output tokens",
            blockReason,
            finishReason,
            response.Usage?.InputTokenCount,
            response.Usage?.OutputTokenCount);

        if (response.FinishReason == ChatFinishReason.Length)
        {
            return new GoogleAiStudioIntegrationException("Google AI Studio ran out of tokens before it wrote an answer.");
        }

        // A normal stop is not a refusal: it only means the model had nothing to say.
        var refusal = blockReason ?? (finishReason == FinishReason.Stop.ToString() ? null : finishReason);

        return refusal is null
            ? new GoogleAiStudioIntegrationException("Google AI Studio returned an empty answer.")
            : new GoogleAiStudioIntegrationException($"Google AI Studio refused to answer ({refusal}).");
    }

    /// <summary>
    /// The exception shown to callers only carries the status, so the provider's own explanation
    /// (quota, billing, region, blocked key, ...) is kept in the log for whoever has to fix it.
    /// </summary>
    private GoogleAiStudioIntegrationException ToFailure(ApiException exception)
    {
        logger.LogWarning(
            "Google AI Studio returned {StatusCode} {Status}: {Message}",
            exception.StatusCode,
            exception.Status,
            Truncate(exception.Message));

        return exception.StatusCode switch
        {
            401 => KeyRejected(),

            // An unknown key is a 400 (INVALID_ARGUMENT) here, and only the reason inside tells it apart.
            400 when IsRejectedKey(exception) => KeyRejected(),

            // Also what an account without access to the free tier gets, and a blocked or leaked key.
            403 => new GoogleAiStudioIntegrationException("Google AI Studio denied access for this API key or project."),
            404 => new GoogleAiStudioIntegrationException("Google AI Studio does not know the configured model."),
            429 => new GoogleAiStudioIntegrationException("The Google AI Studio quota or rate limit is reached. Try again later."),
            _ => new GoogleAiStudioIntegrationException(
                $"Google AI Studio returned {exception.StatusCode}{(string.IsNullOrWhiteSpace(exception.Status) ? string.Empty : $" ({exception.Status})")}."),
        };
    }

    private static GoogleAiStudioIntegrationException KeyRejected() =>
        new("Google AI Studio rejected the API key.");

    private static bool IsRejectedKey(ApiException exception) =>
        exception.Message.Contains("API_KEY_INVALID", StringComparison.OrdinalIgnoreCase)
        || exception.Message.Contains("API key not valid", StringComparison.OrdinalIgnoreCase);

    private static string? Truncate(string? value) =>
        value is { Length: > MaxLoggedErrorLength } ? value[..MaxLoggedErrorLength] : value;
}
