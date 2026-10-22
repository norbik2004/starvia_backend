using System.Net.Http.Json;
using System.Text.Json;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Shared.Infrastructure.Integrations;

public static class HttpResponseMessageExtensions
{
    /// <summary>
    /// Turns a failed response from another API into a <see cref="BusinessException"/> according to
    /// <paramref name="rules"/>. The error body is only read when no status rule matched, and a body that
    /// is missing or not in the standard <see cref="ErrorResponse"/> shape simply falls through to the fallback.
    /// </summary>
    public static async Task<BusinessException> ToFailureAsync(
        this HttpResponseMessage response,
        IntegrationFailureRules rules,
        CancellationToken cancellationToken = default)
    {
        if (rules.MatchStatus(response.StatusCode) is { } byStatus)
        {
            return byStatus;
        }

        var codes = await ReadErrorCodesAsync(response, cancellationToken);
        if (rules.MatchCodes(codes) is { } byCode)
        {
            return byCode;
        }

        return rules.CreateFallback(response.StatusCode);
    }

    private static async Task<IReadOnlySet<string>> ReadErrorCodesAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken);

            // The body comes from a remote service, so its shape is not trusted.
            return (error?.Errors ?? [])
                .Where(item => item?.Code is not null)
                .Select(item => item.Code)
                .ToHashSet();
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            // Not JSON (e.g. an HTML error page from a proxy) or malformed JSON.
            return new HashSet<string>();
        }
    }
}
