using FluentAssertions;
using StarviaBackend.Modules.Accounts.Core.Users.Access;
using StarviaBackend.Modules.Accounts.Core.Users.Exceptions;
using StarviaBackend.Modules.Accounts.Tests.Unit.Fakes;
using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Accounts.Tests.Unit.Access;

public sealed class AccountAccessPolicyTests
{
    private static AccountAction[] AllActions => Enum.GetValues<AccountAction>();

    [Fact]
    public void ScopeFor_AnAdmin_CoversEveryAccountForEveryAction()
    {
        foreach (var action in AllActions)
        {
            var scope = AccountAccessPolicy.ScopeFor(action, new AccessActor(Guid.NewGuid(), IsAdmin: true));

            scope.Allows(Guid.NewGuid().ToString()).Should().BeTrue($"admins may {action}");
        }
    }

    [Fact]
    public void ScopeFor_APlainUser_CoversOnlyTheirOwnAccountForSingleAccountActions()
    {
        var userId = Guid.NewGuid();

        foreach (var action in new[] { AccountAction.ViewAccount, AccountAction.UpdateAccount })
        {
            var scope = AccountAccessPolicy.ScopeFor(action, new AccessActor(userId, IsAdmin: false));

            scope.Allows(userId.ToString()).Should().BeTrue($"users may {action} themselves");
            scope.Allows(Guid.NewGuid().ToString()).Should().BeFalse($"users may not {action} someone else");
        }
    }

    [Fact]
    public void ScopeFor_BrowsingUsers_IsAdminOnly()
    {
        var plainUser = AccountAccessPolicy.ScopeFor(AccountAction.BrowseUsers, new AccessActor(Guid.NewGuid(), IsAdmin: false));
        var admin = AccountAccessPolicy.ScopeFor(AccountAction.BrowseUsers, new AccessActor(Guid.NewGuid(), IsAdmin: true));

        plainUser.IsEmpty.Should().BeTrue();
        admin.Unrestricted.Should().BeTrue();
    }

    [Fact]
    public void ScopeFor_AnAnonymousCaller_CoversNothingForEveryAction()
    {
        foreach (var action in AllActions)
        {
            AccountAccessPolicy.ScopeFor(action, AccessActor.Anonymous).IsEmpty.Should().BeTrue($"anonymous callers may not {action}");
        }
    }
}

public sealed class AccountAccessServiceTests
{
    [Fact]
    public void EnsureAllowed_ForTheAccountOwnerAndForAnAdmin_DoesNotThrow()
    {
        var owner = Guid.NewGuid();

        var asOwner = () => TestAccess.For(owner).EnsureAllowed(AccountAction.ViewAccount, owner);
        var asAdmin = () => TestAccess.Admin().EnsureAllowed(AccountAction.UpdateAccount, owner);

        asOwner.Should().NotThrow();
        asAdmin.Should().NotThrow();
    }

    [Fact]
    public void EnsureAllowed_ForAnotherUserOrAnonymous_ThrowsAccessDenied()
    {
        var accountId = Guid.NewGuid();

        var asStranger = () => TestAccess.For(Guid.NewGuid()).EnsureAllowed(AccountAction.ViewAccount, accountId);
        var asAnonymous = () => TestAccess.Anonymous().EnsureAllowed(AccountAction.UpdateAccount, accountId);

        asStranger.Should().Throw<AccountAccessDeniedException>().Which.Code.Should().Be("account_access_denied");
        asAnonymous.Should().Throw<AccountAccessDeniedException>();
    }

    [Fact]
    public void RequireScopeFor_BrowseUsers_ThrowsForNonAdminsAndReturnsEverythingForAdmins()
    {
        var asUser = () => TestAccess.For(Guid.NewGuid()).RequireScopeFor(AccountAction.BrowseUsers);
        var asAnonymous = () => TestAccess.Anonymous().RequireScopeFor(AccountAction.BrowseUsers);

        asUser.Should().Throw<AccountAccessDeniedException>();
        asAnonymous.Should().Throw<AccountAccessDeniedException>();
        TestAccess.Admin().RequireScopeFor(AccountAction.BrowseUsers).Unrestricted.Should().BeTrue();
    }
}