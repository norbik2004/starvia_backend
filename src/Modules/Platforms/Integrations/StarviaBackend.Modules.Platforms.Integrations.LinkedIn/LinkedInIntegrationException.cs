using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Platforms.Integrations.LinkedIn;

/// <summary>Wraps LinkedIn API failures in a domain exception the error middleware understands.</summary>
public sealed class LinkedInIntegrationException(string message) : BusinessException(message)
{
    public override string Code => "linkedin_integration_failed";
}
