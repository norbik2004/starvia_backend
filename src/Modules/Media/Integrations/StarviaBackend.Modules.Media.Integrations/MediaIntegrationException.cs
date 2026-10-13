using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Media.Integrations;

/// <summary>Wraps media API failures in a domain exception the error middleware understands.</summary>
public sealed class MediaIntegrationException(string message) : BusinessException(message)
{
    public override string Code => "media_integration_failed";
}

public sealed class MediaIntegrationNotOwnerException(string message) : BusinessException(message)
{
    public override string Code => "media_file_not_owned";
}
