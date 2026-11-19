using FluentAssertions;
using StarviaBackend.Modules.Platforms.Application.LinkedIn.Commands.ConnectLinkedInAccount;
using StarviaBackend.Modules.Platforms.Application.LinkedIn.Commands.GetLinkedInAuthorizeUrl;
using StarviaBackend.Modules.Platforms.Core.Platforms.Exceptions;
using StarviaBackend.Modules.Platforms.Integrations.LinkedIn;
using StarviaBackend.Modules.Platforms.Tests.Unit.Fakes;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Platforms.Tests.Unit.Application;

public sealed class LinkedInOAuthStateTests
{
    [Fact]
    public void Create_And_TryParse_RoundTrip()
    {
        var userId = Guid.NewGuid();
        var platformId = Guid.NewGuid();

        var state = LinkedInOAuthState.Create(userId, platformId);

        LinkedInOAuthState.TryParse(state, out var parsedUserId, out var parsedPlatformId).Should().BeTrue();
        parsedUserId.Should().Be(userId);
        parsedPlatformId.Should().Be(platformId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-state")]
    [InlineData("only-one-part")]
    public void TryParse_Rejects_Invalid_State(string? state)
    {
        LinkedInOAuthState.TryParse(state, out _, out _).Should().BeFalse();
    }
}

public sealed class GetLinkedInAuthorizeUrlHandlerTests
{
    private readonly InMemoryPlatformRepository _platforms = new();
    private readonly FakeLinkedInClient _linkedIn = new();

    private GetLinkedInAuthorizeUrlHandler CreateHandler() => new(_platforms, _linkedIn);

    [Fact]
    public async Task HandleAsync_Returns_Authorization_Url_When_LinkedIn_Is_Online()
    {
        var platform = _platforms.Seed(PlatformType.LinkedIn, isOnline: true);
        var userId = Guid.NewGuid();

        var result = await CreateHandler().HandleAsync(new GetLinkedInAuthorizeUrlCommand(userId));

        result.Url.Should().Contain(userId.ToString());
        result.Url.Should().Contain(platform.Id.ToString());
    }

    [Fact]
    public async Task HandleAsync_Throws_When_LinkedIn_Is_Offline()
    {
        _platforms.Seed(PlatformType.LinkedIn, isOnline: false);

        var act = () => CreateHandler().HandleAsync(new GetLinkedInAuthorizeUrlCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<PlatformOfflineException>();
    }
}

public sealed class ConnectLinkedInAccountHandlerTests
{
    private readonly InMemoryPlatformRepository _platforms = new();
    private readonly InMemoryUserPlatformRepository _userPlatforms = new();
    private readonly FakeLinkedInClient _linkedIn = new();

    private ConnectLinkedInAccountHandler CreateHandler() => new(_platforms, _userPlatforms, _linkedIn);

    [Fact]
    public async Task HandleAsync_Stores_Connected_Account_With_Token()
    {
        var platform = _platforms.Seed(PlatformType.LinkedIn, isOnline: true);
        var userId = Guid.NewGuid();
        var state = LinkedInOAuthState.Create(userId, platform.Id);

        var result = await CreateHandler().HandleAsync(new ConnectLinkedInAccountCommand("auth-code", state));

        var stored = _userPlatforms.UserPlatforms.Should().ContainSingle().Subject;
        result.UserPlatformId.Should().Be(stored.Id);
        result.SuccessRedirectUrl.Should().Be(_linkedIn.SuccessRedirectUrl);
        stored.UserId.Should().Be(userId);
        stored.AccessToken.Should().Be("access-token");
        stored.ExternalAccountId.Should().Be("sub-123");
        stored.AccountUsername.Should().Be("Jane Doe");
        stored.ProfilePictureLink.Should().Be("https://example.com/p.png");
    }

    [Fact]
    public async Task HandleAsync_Throws_On_Invalid_State()
    {
        var act = () => CreateHandler().HandleAsync(new ConnectLinkedInAccountCommand("code", "bad"));

        await act.Should().ThrowAsync<LinkedInOAuthStateException>();
    }
}
