using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Platforms.Api.Endpoints.Platforms;

/// <summary>Centralized route/tag/role constants for the User platforms feature endpoints.</summary>
internal static class PlatformsEndpoint
{
    public const string BasePath = "v1/platforms";
    public const string Tag = "Platforms";

    public const string AdminRole = UserRoles.Admin;
}
