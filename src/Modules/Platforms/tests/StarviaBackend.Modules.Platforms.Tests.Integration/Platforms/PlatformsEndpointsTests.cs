using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Initializers;

namespace StarviaBackend.Modules.Platforms.Tests.Integration.Platforms;

public sealed class PlatformsEndpointsTests(PlatformsApp app) : PlatformsIntegrationTest(app)
{
    // --- platforms catalogue -----------------------------------------------------------------

    [Fact]
    public async Task Browse_platforms_returns_the_seeded_catalogue_ordered_by_type()
    {
        // Act
        var platforms = await GetPlatformsAsync(CreateUserClient());

        // Assert
        platforms.Select(p => p.PlatformType).Should().Equal(LinkedIn, Facebook, Instagram);
        platforms.Single(p => p.PlatformType == Facebook).IsOnline.Should().BeTrue();
        platforms.Single(p => p.PlatformType == Instagram).IsOnline.Should().BeFalse();
        platforms.Single(p => p.PlatformType == LinkedIn).IsOnline.Should().BeFalse();
        platforms.Select(p => p.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task Browse_platforms_pages_the_results()
    {
        // Arrange
        var client = CreateUserClient();

        // Act
        var first = await client.GetFromJsonAsync<PagedResponse<PlatformResponse>>("/v1/platforms?page=1&pageSize=2");
        var second = await client.GetFromJsonAsync<PagedResponse<PlatformResponse>>("/v1/platforms?page=2&pageSize=2");

        // Assert
        first!.TotalCount.Should().Be(3);
        first.Items.Should().HaveCount(2);
        second!.Items.Should().ContainSingle();
        second.Items.Single().PlatformType.Should().Be(Instagram);
    }

    [Fact]
    public async Task Seeding_the_platforms_twice_does_not_duplicate_them()
    {
        // Act
        await WithDbAsync<PlatformsWriteDbContext>(async db =>
            await PlatformsSeeder.SeedPlatformsAsync(db, CancellationToken.None));

        // Assert
        var count = await WithDbAsync<PlatformsWriteDbContext, int>(db => db.Set<Platform>().CountAsync());
        count.Should().Be(3);
    }

    // --- user platforms ----------------------------------------------------------------------

    [Fact]
    public async Task Add_user_platform_returns_created_and_stores_the_account_for_the_caller()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        var platformId = await GetPlatformIdAsync(client, Facebook);

        // Act
        var userPlatformId = await AddUserPlatformAsync(client, platformId, "janedoe", "Work account");

        // Assert
        var stored = await WithDbAsync<PlatformsWriteDbContext, UserPlatform>(db =>
            db.Set<UserPlatform>().SingleAsync(u => u.Id == userPlatformId));
        stored.UserId.Should().Be(userId);
        stored.PlatformId.Should().Be(platformId);
        stored.AccountUsername.Should().Be("janedoe");
        stored.AccountComment.Should().Be("Work account");
        stored.CreatedBy.Should().Be(userId.ToString());
    }

    [Fact]
    public async Task Add_user_platform_without_a_comment_is_allowed()
    {
        // Arrange
        var client = CreateUserClient();
        var platformId = await GetPlatformIdAsync(client, LinkedIn);

        // Act
        var response = await PostUserPlatformAsync(client, platformId, "janedoe");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Add_user_platform_for_unknown_platform_returns_platform_not_found()
    {
        // Act
        var response = await PostUserPlatformAsync(Client, Guid.NewGuid(), "janedoe");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "platform_not_found");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("jane123")]
    [InlineData("jane doe")]
    [InlineData("abcdefghijklmnopqrstuvwxyz")]
    public async Task Add_user_platform_with_invalid_username_returns_bad_request(string userName)
    {
        // Arrange
        var platformId = await GetPlatformIdAsync(Client, Facebook);

        // Act
        var response = await PostUserPlatformAsync(Client, platformId, userName);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Add_user_platform_with_too_long_comment_returns_bad_request()
    {
        // Arrange
        var platformId = await GetPlatformIdAsync(Client, Facebook);

        // Act
        var response = await PostUserPlatformAsync(Client, platformId, "janedoe", new string('c', 251));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Add_user_platform_without_identity_returns_unauthorized()
    {
        // Arrange
        var platformId = await GetPlatformIdAsync(Client, Facebook);

        // Act
        var response = await PostUserPlatformAsync(App.CreateClient(), platformId, "janedoe");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Browse_user_platforms_requires_admin_role()
    {
        // Act
        var asUser = await CreateUserClient().GetAsync("/v1/user-platforms");
        var asAdmin = await CreateAdminClient().GetAsync("/v1/user-platforms");

        // Assert
        asUser.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        asAdmin.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Browse_user_platforms_lists_accounts_with_their_platform_type()
    {
        // Arrange
        var client = CreateUserClient();
        var platformId = await GetPlatformIdAsync(client, Instagram);
        var userPlatformId = await AddUserPlatformAsync(client, platformId, "browsableaccount", "Listed");

        // Act
        var page = await CreateAdminClient()
            .GetFromJsonAsync<PagedResponse<UserPlatformResponse>>("/v1/user-platforms?pageSize=100");

        // Assert
        var listed = page!.Items.Should().ContainSingle(u => u.Id == userPlatformId).Subject;
        listed.PlatformType.Should().Be(Instagram);
        listed.AccountUsername.Should().Be("browsableaccount");
        listed.AccountComment.Should().Be("Listed");
        listed.ExternalAccountId.Should().BeNull();
        listed.ProfilePictureLink.Should().BeNull();
    }

    [Fact]
    public async Task Browse_user_platforms_pages_the_results()
    {
        // Arrange
        var client = CreateUserClient();
        var platformId = await GetPlatformIdAsync(client, Facebook);
        await AddUserPlatformAsync(client, platformId, "firstpageaccount");
        await AddUserPlatformAsync(client, platformId, "secondpageaccount");
        var admin = CreateAdminClient();

        // Act
        var page = await admin.GetFromJsonAsync<PagedResponse<UserPlatformResponse>>("/v1/user-platforms?page=1&pageSize=1");

        // Assert
        page!.Items.Should().ContainSingle();
        page.PageSize.Should().Be(1);
        page.TotalCount.Should().BeGreaterThanOrEqualTo(2);
    }
}
