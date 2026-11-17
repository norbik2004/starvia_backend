using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Brands.Core.BrandProfiles.Access;

internal enum BrandAction
{
    Browse,
    View,
    Create,
    Update,
    Delete,
    LinkUserPlatforms,
}

/// <summary>
/// Access rules for brand profiles. Handlers ask <c>IBrandAccessService</c>, which asks this policy.
/// </summary>
internal static class BrandAccessPolicy
{
    public static AccessScope ScopeFor(BrandAction action, AccessActor actor) => action switch
    {
        BrandAction.Browse => actor.OwnerOrAdmin(),
        BrandAction.View => actor.OwnerOrAdmin(),
        BrandAction.Create => actor.Authenticated(),
        BrandAction.Update => actor.OwnerOrAdmin(),
        BrandAction.Delete => actor.OwnerOrAdmin(),
        BrandAction.LinkUserPlatforms => actor.OwnerOrAdmin(),
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "No access rule defined for this action."),
    };
}
