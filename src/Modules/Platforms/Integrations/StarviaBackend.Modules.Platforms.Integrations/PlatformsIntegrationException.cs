using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Platforms.Integrations;

public sealed class PlatformsIntegrationException(string message) : BusinessException(message)
{
    public override string Code => "platforms_integration_failed";
}

public sealed class PlatformsIntegrationNotOwnerException(string message) : BusinessException(message)
{
    public override string Code => "user_platform_not_owned";
}
