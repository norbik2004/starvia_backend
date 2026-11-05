using FluentAssertions;
using StarviaBackend.Modules.Media.Core.Media.Access;
using StarviaBackend.Modules.Media.Core.Media.Exceptions;
using StarviaBackend.Modules.Media.Tests.Unit.Fakes;
using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Media.Tests.Unit.Access;

public sealed class MediaAccessPolicyTests
{
    private static MediaAction[] AllActions => Enum.GetValues<MediaAction>();

    [Fact]
    public void ScopeFor_AnAdmin_CoversEveryFileForEveryAction()
    {
        foreach (var action in AllActions)
        {
            var scope = MediaAccessPolicy.ScopeFor(action, new AccessActor(Guid.NewGuid(), IsAdmin: true));

            scope.Allows(Guid.NewGuid().ToString()).Should().BeTrue($"admins may {action}");
        }
    }

    [Fact]
    public void ScopeFor_APlainUser_CoversOnlyTheirOwnFilesForEveryAction()
    {
        var userId = Guid.NewGuid();

        foreach (var action in AllActions)
        {
            var scope = MediaAccessPolicy.ScopeFor(action, new AccessActor(userId, IsAdmin: false));

            scope.Allows(userId.ToString()).Should().BeTrue($"owners may {action}");
            scope.Allows(Guid.NewGuid().ToString()).Should().BeFalse($"strangers may not {action}");
            scope.Allows(null).Should().BeFalse();
        }
    }

    [Fact]
    public void ScopeFor_AnAnonymousCaller_CoversNothingForEveryAction()
    {
        foreach (var action in AllActions)
        {
            var scope = MediaAccessPolicy.ScopeFor(action, AccessActor.Anonymous);

            scope.IsEmpty.Should().BeTrue($"anonymous callers may not {action}");
        }
    }
}

public sealed class MediaAccessServiceTests
{
    [Fact]
    public void EnsureAllowed_ForTheOwnerAndForAnAdmin_DoesNotThrow()
    {
        var owner = Guid.NewGuid();

        var asOwner = () => TestAccess.For(owner).EnsureAllowed(MediaAction.View, Guid.NewGuid(), owner.ToString());
        var asAdmin = () => TestAccess.Admin().EnsureAllowed(MediaAction.Delete, Guid.NewGuid(), owner.ToString());

        asOwner.Should().NotThrow();
        asAdmin.Should().NotThrow();
    }

    [Fact]
    public void EnsureAllowed_ForAnotherUserOrAnonymous_ThrowsAccessDenied()
    {
        var fileId = Guid.NewGuid();
        var ownerId = Guid.NewGuid().ToString();

        var asStranger = () => TestAccess.For(Guid.NewGuid()).EnsureAllowed(MediaAction.Download, fileId, ownerId);
        var asAnonymous = () => TestAccess.Anonymous().EnsureAllowed(MediaAction.Update, fileId, ownerId);

        asStranger.Should().Throw<MediaAccessDeniedException>().Which.Code.Should().Be("media_access_denied");
        asAnonymous.Should().Throw<MediaAccessDeniedException>();
    }
}

