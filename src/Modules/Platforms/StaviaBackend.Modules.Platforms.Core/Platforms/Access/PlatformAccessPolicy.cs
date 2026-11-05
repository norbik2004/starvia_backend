using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Platforms.Core.Platforms.Access;

/// <summary>
/// Everything a caller can try to do in the Platforms module. Adding an account (a user platform)
/// is open to any authenticated user and the account then belongs to them, so it is not listed.
/// Add a value here, then give it a rule in <see cref="PlatformAccessPolicy"/>.
/// </summary>
internal enum PlatformAction
{
    /// <summary>Reference data: the catalogue of supported platforms.</summary>
    BrowseCatalog,
    BrowseUserPlatforms,
    ViewUserPlatform,
}

/// <summary>
/// THE place where access rules for the Platforms module are defined. Nothing else in the module
/// compares a user id to an owner or looks at roles: handlers and queries ask
/// <c>IPlatformAccessService</c>, which asks this policy.
/// </summary>
internal static class PlatformAccessPolicy
{
    public static AccessScope ScopeFor(PlatformAction action, AccessActor actor) => action switch
    {
        // The catalogue is public reference data (also readable without logging in).
        PlatformAction.BrowseCatalog => AccessScope.All,
        PlatformAction.BrowseUserPlatforms => actor.OwnerOrAdmin(),
        PlatformAction.ViewUserPlatform => actor.OwnerOrAdmin(),
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "No access rule defined for this action."),
    };
}
