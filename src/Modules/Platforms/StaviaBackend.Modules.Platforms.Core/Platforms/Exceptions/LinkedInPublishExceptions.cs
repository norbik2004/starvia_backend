using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Platforms.Core.Platforms.Exceptions;

internal sealed class PostBodyRequiredException()
    : BusinessException("Post body cannot be empty when publishing.")
{
    public override string Code => "post_body_required";
}

internal sealed class UserPlatformNotConnectedException(Guid userPlatformId)
    : BusinessException($"User platform '{userPlatformId}' is missing an access token or external account id.")
{
    public override string Code => "user_platform_not_connected";
}

internal sealed class UserPlatformPlatformMismatchException(Guid userPlatformId, string expected)
    : BusinessException($"User platform '{userPlatformId}' is not a {expected} account.")
{
    public override string Code => "user_platform_type_mismatch";
}
