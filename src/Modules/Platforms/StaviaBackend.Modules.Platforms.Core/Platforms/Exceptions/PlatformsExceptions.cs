using StarviaBackend.Shared.Abstractions.Exceptions;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Platforms.Core.Platforms.Exceptions;

internal sealed class PlatformNotFoundException : BusinessException
{
    public PlatformNotFoundException(Guid platformId)
        : base($"Platform '{platformId}' was not found.")
    {
    }

    public PlatformNotFoundException(PlatformType platformType)
        : base($"Platform '{platformType}' was not found.")
    {
    }

    public override string Code => "platform_not_found";
}

internal sealed class PlatformOfflineException(PlatformType platformType)
    : BusinessException($"Platform '{platformType}' is currently offline.")
{
    public override string Code => "platform_offline";
}

internal sealed class LinkedInOAuthStateException()
    : BusinessException("Missing or invalid LinkedIn OAuth state parameter.")
{
    public override string Code => "linkedin_oauth_state_invalid";
}

