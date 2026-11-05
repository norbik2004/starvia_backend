using StarviaBackend.Modules.Platforms.Core.Platforms.Access;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Platforms.Core.Platforms.Exceptions;

internal sealed class UserPlatformNotFoundException(Guid userPlatformId)
    : BusinessException($"User platform '{userPlatformId}' was not found.")
{
    public override string Code => "user_platform_not_found";
}

internal sealed class UserPlatformAccessDeniedException(Guid userPlatformId, PlatformAction action)
    : ForbiddenException($"You are not allowed to {action} on user platform '{userPlatformId}'.")
{
    public override string Code => "user_platform_access_denied";
}
