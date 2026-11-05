using StarviaBackend.Shared.Abstractions.Auth;
using FluentAssertions;
using StarviaBackend.Modules.Posts.Core.Posts.Access;
using StarviaBackend.Modules.Posts.Core.Posts.Exceptions;
using StarviaBackend.Modules.Posts.Tests.Unit.Fakes;

namespace StarviaBackend.Modules.Posts.Tests.Unit.Access;

public sealed class PostAccessPolicyTests
{
    private static PostAction[] AllActions => Enum.GetValues<PostAction>();

    [Fact]
    public void ScopeFor_AnAdmin_CoversEveryPostForEveryAction()
    {
        foreach (var action in AllActions)
        {
            // Act
            var scope = PostAccessPolicy.ScopeFor(action, new AccessActor(Guid.NewGuid(), IsAdmin: true));

            // Assert
            scope.Unrestricted.Should().BeTrue($"admins may {action}");
            scope.Allows(Guid.NewGuid().ToString()).Should().BeTrue($"admins may {action}");
        }
    }

    [Fact]
    public void ScopeFor_APlainUser_CoversOnlyTheirOwnPostsForEveryAction()
    {
        // Arrange
        var userId = Guid.NewGuid();

        foreach (var action in AllActions)
        {
            // Act
            var scope = PostAccessPolicy.ScopeFor(action, new AccessActor(userId, IsAdmin: false));

            // Assert
            scope.Unrestricted.Should().BeFalse($"users are limited for {action}");
            scope.OwnerId.Should().Be(userId.ToString());
            scope.Allows(userId.ToString()).Should().BeTrue();
            scope.Allows(Guid.NewGuid().ToString()).Should().BeFalse();
            scope.Allows(null).Should().BeFalse();
        }
    }

    [Fact]
    public void ScopeFor_AnAnonymousCaller_CoversNothingForEveryAction()
    {
        foreach (var action in AllActions)
        {
            // Act
            var scope = PostAccessPolicy.ScopeFor(action, new AccessActor(null, IsAdmin: false));

            // Assert
            scope.Allows(Guid.NewGuid().ToString()).Should().BeFalse($"anonymous callers may not {action}");
            scope.Allows(null).Should().BeFalse($"anonymous callers may not {action}");
        }
    }
}
public sealed class PostAccessServiceTests
{
    [Fact]
    public void EnsureAllowed_ForTheOwner_DoesNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var access = TestAccess.For(userId);

        // Act
        var act = () => access.EnsureAllowed(PostAction.View, Guid.NewGuid(), userId.ToString());

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureAllowed_ForAnAdmin_DoesNotThrowForSomeoneElsesPost()
    {
        // Act
        var act = () => TestAccess.Admin().EnsureAllowed(PostAction.Delete, Guid.NewGuid(), Guid.NewGuid().ToString());

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureAllowed_ForAnotherUser_ThrowsAccessDenied()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var access = TestAccess.For(Guid.NewGuid());

        // Act
        var act = () => access.EnsureAllowed(PostAction.Update, postId, Guid.NewGuid().ToString());

        // Assert
        var exception = act.Should().Throw<PostAccessDeniedException>().Which;
        exception.Code.Should().Be("post_access_denied");
        exception.Message.Should().Contain(postId.ToString());
    }

    [Fact]
    public void EnsureAllowed_ForAnAnonymousCaller_ThrowsAccessDenied()
    {
        // Act
        var act = () => TestAccess.Anonymous().EnsureAllowed(PostAction.View, Guid.NewGuid(), Guid.NewGuid().ToString());

        // Assert
        act.Should().Throw<PostAccessDeniedException>();
    }
}
