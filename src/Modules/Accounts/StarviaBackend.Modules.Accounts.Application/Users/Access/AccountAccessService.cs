using StarviaBackend.Modules.Accounts.Core.Users.Access;
using StarviaBackend.Modules.Accounts.Core.Users.Exceptions;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Contexts;

namespace StarviaBackend.Modules.Accounts.Application.Users.Access;

/// <summary>
/// How handlers and queries consult <see cref="AccountAccessPolicy"/> for the caller of the
/// current request. Holds no rules itself.
/// </summary>
internal interface IAccountAccessService
{
    /// <summary>The accounts the current caller may use for <paramref name="action"/>.</summary>
    AccessScope ScopeFor(AccountAction action);

    /// <summary>
    /// Like <see cref="ScopeFor"/> for listings the caller must not even see as empty (e.g. admin-only
    /// ones): throws <see cref="AccountAccessDeniedException"/> when the scope covers nothing.
    /// </summary>
    AccessScope RequireScopeFor(AccountAction action);

    /// <summary>Throws <see cref="AccountAccessDeniedException"/> unless the caller may do <paramref name="action"/> on the account of <paramref name="accountUserId"/>.</summary>
    void EnsureAllowed(AccountAction action, Guid accountUserId);
}

internal sealed class AccountAccessService(IContext context) : IAccountAccessService
{
    public AccessScope ScopeFor(AccountAction action) =>
        AccountAccessPolicy.ScopeFor(action, AccessActor.From(context.Identity));

    public AccessScope RequireScopeFor(AccountAction action)
    {
        var scope = ScopeFor(action);
        if (scope.IsEmpty)
        {
            throw new AccountAccessDeniedException(null, action);
        }

        return scope;
    }

    public void EnsureAllowed(AccountAction action, Guid accountUserId)
    {
        if (!ScopeFor(action).Allows(accountUserId.ToString()))
        {
            throw new AccountAccessDeniedException(accountUserId, action);
        }
    }
}