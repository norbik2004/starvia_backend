using StarviaBackend.Modules.Brands.Core.BrandProfiles.Access;
using StarviaBackend.Shared.Abstractions.Exceptions;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Brands.Core.BrandProfiles.Exceptions;

internal sealed class BrandProfileNotFoundException(Guid brandProfileId)
    : BusinessException($"Brand profile '{brandProfileId}' was not found")
{
    public override string Code => "brand_profile_not_found";
}

internal sealed class BrandProfileAccessDeniedException(Guid brandProfileId, BrandAction action)
    : ForbiddenException($"You are not allowed to {action} brand profile '{brandProfileId}'")
{
    public override string Code => "brand_profile_access_denied";
}

internal sealed class DuplicateUserPlatformLinkException(Guid userPlatformId)
    : BusinessException($"User platform '{userPlatformId}' is linked more than once")
{
    public override string Code => "duplicate_user_platform_link";
}

internal sealed class DuplicatePlatformTypeException(PlatformType platformType)
    : BusinessException($"Platform type '{platformType}' is selected more than once")
{
    public override string Code => "duplicate_platform_type";
}
