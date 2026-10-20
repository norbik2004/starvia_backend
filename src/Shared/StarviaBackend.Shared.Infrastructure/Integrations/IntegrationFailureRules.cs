using System.Net;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Shared.Infrastructure.Integrations;

/// <summary>
/// Describes how a failed response from an external API is translated into a <see cref="BusinessException"/>.
/// Rules are evaluated in the order they were added: HTTP status rules first, then rules matching an error
/// <c>code</c> from the standard <see cref="ErrorResponse"/> body. Anything unmatched goes to the fallback.
/// </summary>
/// <param name="fallback">Creates the exception when no status or code rule matches.</param>
public sealed class IntegrationFailureRules(Func<HttpStatusCode, BusinessException> fallback)
{
    private readonly List<(HttpStatusCode Status, Func<BusinessException> Create)> _statusRules = [];
    private readonly List<(string Code, Func<BusinessException> Create)> _codeRules = [];

    /// <summary>Maps responses with the given HTTP status code.</summary>
    public IntegrationFailureRules OnStatus(HttpStatusCode status, Func<BusinessException> create)
    {
        _statusRules.Add((status, create));
        return this;
    }

    /// <summary>Maps responses whose error body contains the given error code.</summary>
    public IntegrationFailureRules OnCode(string code, Func<BusinessException> create)
    {
        _codeRules.Add((code, create));
        return this;
    }

    internal BusinessException? MatchStatus(HttpStatusCode status)
    {
        foreach (var (ruleStatus, create) in _statusRules)
        {
            if (ruleStatus == status)
            {
                return create();
            }
        }

        return null;
    }

    internal BusinessException? MatchCodes(IReadOnlySet<string> codes)
    {
        foreach (var (ruleCode, create) in _codeRules)
        {
            if (codes.Contains(ruleCode))
            {
                return create();
            }
        }

        return null;
    }

    internal BusinessException CreateFallback(HttpStatusCode status) => fallback(status);
}
