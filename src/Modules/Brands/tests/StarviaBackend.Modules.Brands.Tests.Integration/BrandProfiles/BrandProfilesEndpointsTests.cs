using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StarviaBackend.Modules.Brands.Application.BrandProfiles;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Models;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Brands.Tests.Integration.BrandProfiles;

public sealed class BrandProfilesEndpointsTests(BrandsApp app) : BrandsIntegrationTest(app)
{
    [Fact]
    public async Task Create_get_update_and_delete_brand_profile_round_trips()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var client = CreateUserClient(ownerId);

        // Act - create
        var id = await CreateBrandProfileAsync(
            client,
            "Acme LinkedIn",
            platforms: [PlatformType.LinkedIn, PlatformType.Instagram]);

        // Assert - get
        var profile = await client.GetFromJsonAsync<BrandProfileResponse>($"/v1/brand-profiles/{id}", JsonOptions);
        profile!.Id.Should().Be(id);
        profile.Name.Should().Be("Acme LinkedIn");
        profile.OwnerId.Should().Be(ownerId);
        profile.Brief.Company!.Name.Should().Be("Acme Studio");
        profile.Platforms.Should().Equal(PlatformType.LinkedIn, PlatformType.Instagram);
        profile.GenerationDefaults!.PostType.Should().Be("Educational");
        profile.UserPlatforms.Should().BeEmpty();

        // Act - update
        var updatedBrief = SampleBrief() with
        {
            Company = new BrandCompany("Acme Studio PL", "Marketing", "Zaktualizowany opis"),
        };
        var updateResponse = await client.PutAsJsonAsync($"/v1/brand-profiles/{id}", new
        {
            Name = "Acme PL",
            Brief = updatedBrief,
            Platforms = new[] { PlatformType.Instagram },
            GenerationDefaults = new BrandGenerationDefaults("Promotional", "Awareness", "Polish"),
        }, JsonOptions);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterUpdate = await client.GetFromJsonAsync<BrandProfileResponse>($"/v1/brand-profiles/{id}", JsonOptions);
        afterUpdate!.Name.Should().Be("Acme PL");
        afterUpdate.Brief.Company!.Name.Should().Be("Acme Studio PL");
        afterUpdate.Platforms.Should().Equal(PlatformType.Instagram);
        afterUpdate.GenerationDefaults!.PostType.Should().Be("Promotional");

        // Act - delete
        var deleteResponse = await client.DeleteAsync($"/v1/brand-profiles/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfterDelete = await client.GetAsync($"/v1/brand-profiles/{id}");
        await AssertErrorCodeAsync(getAfterDelete, HttpStatusCode.BadRequest, "brand_profile_not_found");
    }

    [Fact]
    public async Task Browse_returns_only_own_profiles_for_a_plain_user()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var owner = CreateUserClient(ownerId);
        var mine = await CreateBrandProfileAsync(owner, "Mine");
        await CreateBrandProfileAsync(CreateUserClient(), "Theirs");

        // Act
        var page = await owner.GetFromJsonAsync<PagedResponse<BrandProfileSummaryResponse>>(
            "/v1/brand-profiles?pageSize=100");

        // Assert
        page!.TotalCount.Should().Be(1);
        page.Items.Select(p => p.Id).Should().Equal(mine);
        page.Items.Should().OnlyContain(p => p.OwnerId == ownerId);
    }

    [Fact]
    public async Task Get_someone_elses_profile_returns_forbidden()
    {
        // Arrange
        var id = await CreateBrandProfileAsync(CreateUserClient());

        // Act
        var response = await CreateUserClient().GetAsync($"/v1/brand-profiles/{id}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.Forbidden, "brand_profile_access_denied");
    }

    [Fact]
    public async Task Link_user_platforms_and_filter_by_account()
    {
        // Arrange
        var owner = CreateUserClient();
        var profileId = await CreateBrandProfileAsync(owner, "Linked");
        var platformA = Guid.NewGuid();
        var platformB = Guid.NewGuid();

        // Act - link
        var linkResponse = await owner.PutAsJsonAsync($"/v1/brand-profiles/{profileId}/user-platforms", new
        {
            UserPlatforms = new[]
            {
                new BrandProfileUserPlatformLink(platformA, true),
                new BrandProfileUserPlatformLink(platformB, false),
            },
        });
        linkResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var profile = await owner.GetFromJsonAsync<BrandProfileResponse>($"/v1/brand-profiles/{profileId}", JsonOptions);
        profile!.UserPlatforms.Should().HaveCount(2);
        profile.UserPlatforms.Should().Contain(l => l.UserPlatformId == platformA && l.IsDefault);
        profile.UserPlatforms.Should().Contain(l => l.UserPlatformId == platformB && !l.IsDefault);

        // Act - browse by platform
        var byQuery = await owner.GetFromJsonAsync<PagedResponse<BrandProfileSummaryResponse>>(
            $"/v1/brand-profiles?userPlatformId={platformA}");
        byQuery!.Items.Select(p => p.Id).Should().Equal(profileId);
        byQuery.Items.Single().LinkedUserPlatformCount.Should().Be(2);

        var byRoute = await owner.GetFromJsonAsync<List<BrandProfileSummaryResponse>>(
            $"/v1/brand-profiles/by-user-platform/{platformA}");
        byRoute!.Select(p => p.Id).Should().Equal(profileId);

        // Act - replace links (clear)
        var clearResponse = await owner.PutAsJsonAsync($"/v1/brand-profiles/{profileId}/user-platforms", new
        {
            UserPlatforms = Array.Empty<BrandProfileUserPlatformLink>(),
        });
        clearResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterClear = await owner.GetFromJsonAsync<BrandProfileResponse>($"/v1/brand-profiles/{profileId}", JsonOptions);
        afterClear!.UserPlatforms.Should().BeEmpty();
    }

    [Fact]
    public async Task Setting_default_clears_default_on_other_profiles_for_same_account()
    {
        // Arrange
        var owner = CreateUserClient();
        var first = await CreateBrandProfileAsync(owner, "First");
        var second = await CreateBrandProfileAsync(owner, "Second");
        var userPlatformId = Guid.NewGuid();

        await owner.PutAsJsonAsync($"/v1/brand-profiles/{first}/user-platforms", new
        {
            UserPlatforms = new[] { new BrandProfileUserPlatformLink(userPlatformId, true) },
        });

        // Act
        await owner.PutAsJsonAsync($"/v1/brand-profiles/{second}/user-platforms", new
        {
            UserPlatforms = new[] { new BrandProfileUserPlatformLink(userPlatformId, true) },
        });

        // Assert
        var firstProfile = await owner.GetFromJsonAsync<BrandProfileResponse>($"/v1/brand-profiles/{first}", JsonOptions);
        var secondProfile = await owner.GetFromJsonAsync<BrandProfileResponse>($"/v1/brand-profiles/{second}", JsonOptions);
        firstProfile!.UserPlatforms.Single().IsDefault.Should().BeFalse();
        secondProfile!.UserPlatforms.Single().IsDefault.Should().BeTrue();
    }
}
