using FluentAssertions;
using StarviaBackend.Modules.Platforms.Core.Platforms.Access;
using StarviaBackend.Modules.Platforms.Core.Platforms.Exceptions;
using StarviaBackend.Modules.Platforms.Tests.Unit.Fakes;
using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Platforms.Tests.Unit.Access;

public sealed class PlatformAccessPolicyTests
{
    private static PlatformAction[] UserPlatformActions =>
        [PlatformAction.BrowseUserPlatforms, PlatformAction.ViewUserPlatform];

    [Fact]
    public void ScopeFor_Catalogue_IsOpenToEverybodyIncludingAnonymousCallers()
    {
        foreach (var actor in new[] { AccessActor.Anonymous, new AccessActor(Guid.NewGuid(), false), new AccessActor(Guid.NewGuid(), true) })
        {
            PlatformAccessPolicy.ScopeFor(PlatformAction.BrowseCatalog, actor).Unrestricted.Should().BeTrue();
        }
    }

    [Fact]
    public void ScopeFor_AnAdmin_CoversEveryUserPlatform()
    {
        foreach (var action in UserPlatformActions)
        {
            var scope = PlatformAccessPolicy.ScopeFor(action, new AccessActor(Guid.NewGuid(), IsAdmin: true));

            scope.Allows(Guid.NewGuid().ToString()).Should().BeTrue($"admins may {action}");
        }
    }

    [Fact]
    public void ScopeFor_APlainUser_CoversOnlyTheirOwnUserPlatforms()
    {
        var userId = Guid.NewGuid();

        foreach (var action in UserPlatformActions)
        {
            var scope = PlatformAccessPolicy.ScopeFor(action, new AccessActor(userId, IsAdmin: false));

            scope.Allows(userId.ToString()).Should().BeTrue($"owners may {action}");
            scope.Allows(Guid.NewGuid().ToString()).Should().BeFalse($"strangers may not {action}");
            scope.Allows(null).Should().BeFalse();
        }
    }

    [Fact]
    public void ScopeFor_AnAnonymousCaller_CoversNoUserPlatform()
    {
        foreach (var action in UserPlatformActions)
        {
            PlatformAccessPolicy.ScopeFor(action, AccessActor.Anonymous).IsEmpty.Should().BeTrue($"anonymous callers may not {action}");
        }
    }
}

public sealed class PlatformAccessServiceTests
{
    [Fact]
    public void EnsureAllowed_ForTheOwnerAndForAnAdmin_DoesNotThrow()
    {
        var owner = Guid.NewGuid();

        var asOwner = () => TestAccess.For(owner).EnsureAllowed(PlatformAction.ViewUserPlatform, Guid.NewGuid(), owner.ToString());
        var asAdmin = () => TestAccess.Admin().EnsureAllowed(PlatformAction.ViewUserPlatform, Guid.NewGuid(), owner.ToString());

        asOwner.Should().NotThrow();
        asAdmin.Should().NotThrow();
    }

    [Fact]
    public void EnsureAllowed_ForAnotherUserOrAnonymous_ThrowsAccessDenied()
    {
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid().ToString();

        var asStranger = () => TestAccess.For(Guid.NewGuid()).EnsureAllowed(PlatformAction.ViewUserPlatform, id, ownerId);
        var asAnonymous = () => TestAccess.Anonymous().EnsureAllowed(PlatformAction.ViewUserPlatform, id, ownerId);

        asStranger.Should().Throw<UserPlatformAccessDeniedException>().Which.Code.Should().Be("user_platform_access_denied");
        asAnonymous.Should().Throw<UserPlatformAccessDeniedException>();
    }
}
