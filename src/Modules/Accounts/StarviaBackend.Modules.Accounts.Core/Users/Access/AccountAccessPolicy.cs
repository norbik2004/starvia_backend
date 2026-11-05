using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Accounts.Core.Users.Access;

/// <summary>
/// Everything a caller can try to do with accounts. An account is its own "owner": the resource
/// owner id used by the rules below is the id of the user the account belongs to. Registering, signing
/// in and the password / e-mail flows work on the caller's own account by definition (or are anonymous
/// by design), so they are not listed. Add a value here, then give it a rule in <see cref="AccountAccessPolicy"/>.
/// </summary>
internal enum AccountAction
{
    /// <summary>Listing every account.</summary>
    BrowseUsers,
    ViewAccount,
    UpdateAccount,
}

/// <summary>
/// THE place where access rules for the Accounts module are defined. Nothing else in the module
/// compares user ids or looks at roles: handlers and queries ask <c>IAccountAccessService</c>, which
/// asks this policy.
/// </summary>
internal static class AccountAccessPolicy
{
    public static AccessScope ScopeFor(AccountAction action, AccessActor actor) => action switch
    {
        AccountAction.BrowseUsers => actor.AdminOnly(),
        AccountAction.ViewAccount => actor.OwnerOrAdmin(),
        AccountAction.UpdateAccount => actor.OwnerOrAdmin(),
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "No access rule defined for this action."),
    };
}