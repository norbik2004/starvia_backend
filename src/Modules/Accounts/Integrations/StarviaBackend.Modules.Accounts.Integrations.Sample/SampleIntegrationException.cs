using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Accounts.Integrations.Sample;

/// <summary>Wraps vendor failures in a domain exception the error middleware understands.</summary>
public sealed class SampleIntegrationException(string message) : BusinessException(message)
{
    public override string Code => "sample_integration_failed";
}
