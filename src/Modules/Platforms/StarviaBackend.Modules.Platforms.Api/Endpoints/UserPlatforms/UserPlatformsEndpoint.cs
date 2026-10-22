using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Platforms.Api.Endpoints.UserPlatforms;

/// <summary>Centralized route/tag/role constants for the User platforms feature endpoints.</summary>
internal static class UserPlatformsEndpoint
{
    public const string BasePath = "v1/user-platforms";
    public const string Tag = "UserPlatforms";

    public const string AdminRole = UserRoles.Admin;
}
