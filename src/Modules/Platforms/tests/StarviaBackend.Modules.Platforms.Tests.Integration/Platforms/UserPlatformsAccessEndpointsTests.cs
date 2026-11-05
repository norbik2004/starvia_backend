using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace StarviaBackend.Modules.Platforms.Tests.Integration.Platforms;

/// <summary>Who may see which connected platform account: the user who connected it and admins.</summary>
public sealed class UserPlatformsAccessEndpointsTests(PlatformsApp app) : PlatformsIntegrationTest(app)
{
    // ---- get by id --------------------------------------------------------------------------

    [Fact]
    public async Task Get_user_platform_returns_it_to_the_owner_and_to_an_admin()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var owner = CreateUserClient(ownerId);
        var id = await AddUserPlatformAsync(owner, await GetPlatformIdAsync(owner, Instagram), "myaccount", "Mine");

        // Act
        var asOwner = await owner.GetFromJsonAsync<UserPlatformResponse>($"/v1/user-platforms/{id}");
        var asAdmin = await CreateAdminClient().GetFromJsonAsync<UserPlatformResponse>($"/v1/user-platforms/{id}");

        // Assert
        asOwner!.AccountUsername.Should().Be("myaccount");
        asOwner.PlatformType.Should().Be(Instagram);
        asOwner.AccountComment.Should().Be("Mine");
        asOwner.OwnerId.Should().Be(ownerId);
        asAdmin!.Id.Should().Be(id);
    }

    [Fact]
    public async Task Get_user_platform_of_someone_else_returns_forbidden()
    {
        // Arrange
        var owner = CreateUserClient();
        var id = await AddUserPlatformAsync(owner, await GetPlatformIdAsync(owner, Facebook), "privateaccount");

        // Act
        var response = await CreateUserClient().GetAsync($"/v1/user-platforms/{id}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.Forbidden, "user_platform_access_denied");
    }

    [Fact]
    public async Task Get_unknown_user_platform_returns_user_platform_not_found()
    {
        // Act
        var response = await CreateUserClient().GetAsync($"/v1/user-platforms/{Guid.NewGuid()}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "user_platform_not_found");
    }

    [Fact]
    public async Task Get_user_platform_without_identity_returns_unauthorized()
    {
        // Act
        var response = await App.CreateClient().GetAsync($"/v1/user-platforms/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ---- browse -----------------------------------------------------------------------------

    [Fact]
    public async Task Browse_user_platforms_as_a_plain_user_returns_only_their_own_accounts()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var owner = CreateUserClient(ownerId);
        var platformId = await GetPlatformIdAsync(owner, LinkedIn);
        var mine = await AddUserPlatformAsync(owner, platformId, "ownaccount");
        await AddUserPlatformAsync(CreateUserClient(), platformId, "otheraccount");

        // Act
        var page = await owner.GetFromJsonAsync<PagedResponse<UserPlatformResponse>>("/v1/user-platforms?pageSize=100");

        // Assert
        page!.TotalCount.Should().Be(1);
        page.Items.Select(u => u.Id).Should().Equal(mine);
        page.Items.Should().OnlyContain(u => u.OwnerId == ownerId);
    }

    [Fact]
    public async Task Browse_user_platforms_by_someone_elses_owner_id_as_a_plain_user_returns_nothing()
    {
        // Arrange
        var otherId = Guid.NewGuid();
        var other = CreateUserClient(otherId);
        await AddUserPlatformAsync(other, await GetPlatformIdAsync(other, LinkedIn), "hiddenaccount");

        // Act
        var page = await CreateUserClient().GetFromJsonAsync<PagedResponse<UserPlatformResponse>>($"/v1/user-platforms?ownerId={otherId}");

        // Assert
        page!.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Browse_user_platforms_as_admin_can_narrow_down_to_one_owner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var owner = CreateUserClient(ownerId);
        var platformId = await GetPlatformIdAsync(owner, Instagram);
        var mine = await AddUserPlatformAsync(owner, platformId, "ownersaccount");
        await AddUserPlatformAsync(CreateUserClient(), platformId, "strangersaccount");

        // Act
        var page = await CreateAdminClient()
            .GetFromJsonAsync<PagedResponse<UserPlatformResponse>>($"/v1/user-platforms?ownerId={ownerId}");

        // Assert
        page!.Items.Select(u => u.Id).Should().Equal(mine);
    }

    [Fact]
    public async Task Browse_user_platforms_filters_by_platform_and_searches_the_account_name()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var owner = CreateUserClient(ownerId);
        var linkedIn = await AddUserPlatformAsync(owner, await GetPlatformIdAsync(owner, LinkedIn), "Johnsmith");
        var facebook = await AddUserPlatformAsync(owner, await GetPlatformIdAsync(owner, Facebook), "Janedoe");

        // Act
        var byPlatform = await owner.GetFromJsonAsync<PagedResponse<UserPlatformResponse>>($"/v1/user-platforms?platformType={Facebook}");
        var byText = await owner.GetFromJsonAsync<PagedResponse<UserPlatformResponse>>("/v1/user-platforms?search=SMITH");
        var none = await owner.GetFromJsonAsync<PagedResponse<UserPlatformResponse>>("/v1/user-platforms?search=nobody");

        // Assert
        byPlatform!.Items.Select(u => u.Id).Should().Equal(facebook);
        byText!.Items.Select(u => u.Id).Should().Equal(linkedIn);
        none!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Browse_user_platforms_sorts_by_account_name_and_pages_past_the_end()
    {
        // Arrange
        var owner = CreateUserClient();
        var platformId = await GetPlatformIdAsync(owner, Facebook);
        var b = await AddUserPlatformAsync(owner, platformId, "bbbaccount");
        var a = await AddUserPlatformAsync(owner, platformId, "aaaaccount");

        // Act
        var ascending = await owner.GetFromJsonAsync<PagedResponse<UserPlatformResponse>>("/v1/user-platforms?sortBy=AccountUsername");
        var descending = await owner.GetFromJsonAsync<PagedResponse<UserPlatformResponse>>("/v1/user-platforms?sortBy=AccountUsername&descending=true");
        var past = await owner.GetFromJsonAsync<PagedResponse<UserPlatformResponse>>("/v1/user-platforms?page=50");

        // Assert
        ascending!.Items.Select(u => u.Id).Should().Equal(a, b);
        descending!.Items.Select(u => u.Id).Should().Equal(b, a);
        past!.Items.Should().BeEmpty();
        past.TotalCount.Should().Be(2);
    }

    // ---- catalogue --------------------------------------------------------------------------

    [Fact]
    public async Task Browse_platforms_filters_by_type_and_online_status_and_is_public()
    {
        // Act
        var anonymous = App.CreateClient();
        var facebook = await anonymous.GetFromJsonAsync<PagedResponse<PlatformResponse>>($"/v1/platforms?platformType={Facebook}");
        var offline = await anonymous.GetFromJsonAsync<PagedResponse<PlatformResponse>>("/v1/platforms?isOnline=false");
        var online = await anonymous.GetFromJsonAsync<PagedResponse<PlatformResponse>>("/v1/platforms?isOnline=true&pageSize=100");

        // Assert
        facebook!.Items.Should().ContainSingle().Which.PlatformType.Should().Be(Facebook);
        offline!.Items.Should().OnlyContain(p => !p.IsOnline);
        online!.Items.Should().OnlyContain(p => p.IsOnline);
    }
}
