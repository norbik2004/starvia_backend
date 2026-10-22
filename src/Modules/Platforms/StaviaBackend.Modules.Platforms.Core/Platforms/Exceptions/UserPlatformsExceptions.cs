using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Platforms.Core.Platforms.Exceptions;

internal sealed class UserPlatformNotFoundException(Guid userPlatformId)
    : BusinessException($"User platform '{userPlatformId}' was not found.")
{
    public override string Code => "user_platform_not_found";
}

