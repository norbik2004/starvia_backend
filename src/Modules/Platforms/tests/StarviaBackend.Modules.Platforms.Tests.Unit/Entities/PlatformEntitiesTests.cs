using FluentAssertions;
using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;
using StarviaBackend.Modules.Platforms.Core.Platforms.Enums;

namespace StarviaBackend.Modules.Platforms.Tests.Unit.Entities;

public sealed class PlatformTests
{
    [Theory]
    [InlineData(PlatformType.LinkedIn, true)]
    [InlineData(PlatformType.Facebook, false)]
    [InlineData(PlatformType.Instagram, true)]
    public void Create_AssignsTypeAndOnlineFlag(PlatformType type, bool isOnline)
    {
        // Act
        var platform = Platform.Create(type, isOnline);

        // Assert
        platform.Id.Should().NotBeEmpty();
        platform.PlatformType.Should().Be(type);
        platform.IsOnline.Should().Be(isOnline);
        platform.UserPlatforms.Should().BeEmpty();
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        Platform.Create(PlatformType.Facebook, true).Id
            .Should().NotBe(Platform.Create(PlatformType.Facebook, true).Id);
    }

    [Fact]
    public void UpdateStatus_TogglesTheOnlineFlag()
    {
        // Arrange
        var platform = Platform.Create(PlatformType.Instagram, false);

        // Act
        platform.UpdateStatus(true);

        // Assert
        platform.IsOnline.Should().BeTrue();
    }
}

public sealed class UserPlatformTests
{
    [Fact]
    public void Create_WithBasicDetails_AssignsThemAndLeavesTheRestEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        // Act
        var userPlatform = UserPlatform.Create(userId, platformId, "janedoe", "My work account");

        // Assert
        userPlatform.Id.Should().NotBeEmpty();
        userPlatform.UserId.Should().Be(userId);
        userPlatform.PlatformId.Should().Be(platformId);
        userPlatform.AccountUsername.Should().Be("janedoe");
        userPlatform.AccountComment.Should().Be("My work account");
        userPlatform.AccessToken.Should().BeNull();
        userPlatform.ExternalAccountId.Should().BeNull();
        userPlatform.ProfilePictureLink.Should().BeNull();
    }

    [Fact]
    public void Create_WithFullDetails_AssignsEverything()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        // Act
        var userPlatform = UserPlatform.Create(
            userId, platformId, "janedoe", "token-123", "external-1", "Comment", "https://pics.test/jane.png");

        // Assert
        userPlatform.UserId.Should().Be(userId);
        userPlatform.PlatformId.Should().Be(platformId);
        userPlatform.AccountUsername.Should().Be("janedoe");
        userPlatform.AccessToken.Should().Be("token-123");
        userPlatform.ExternalAccountId.Should().Be("external-1");
        userPlatform.AccountComment.Should().Be("Comment");
        userPlatform.ProfilePictureLink.Should().Be("https://pics.test/jane.png");
    }

    [Fact]
    public void Create_AllowsMissingComment()
    {
        // Act
        var userPlatform = UserPlatform.Create(Guid.NewGuid(), Guid.NewGuid(), "janedoe", null);

        // Assert
        userPlatform.AccountComment.Should().BeNull();
    }

    [Fact]
    public void UpdateToken_ReplacesTheAccessToken()
    {
        // Arrange
        var userPlatform = UserPlatform.Create(Guid.NewGuid(), Guid.NewGuid(), "janedoe", "old-token", null, null, null);

        // Act
        userPlatform.UpdateToken("new-token");

        // Assert
        userPlatform.AccessToken.Should().Be("new-token");
    }
}
