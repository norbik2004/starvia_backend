using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Models;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Exceptions;
using StarviaBackend.Shared.Abstractions.Platforms;
using StarviaBackend.Shared.Infrastructure.Serialization;
using StarviaBackend.Shared.Tests.Integration;

namespace StarviaBackend.Modules.Brands.Tests.Integration;

[Collection(BrandsCollection.Name)]
public abstract class BrandsIntegrationTest(BrandsApp app) : IntegrationTest<BrandsApp>(app)
{
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new PlatformTypeJsonConverter() },
    };

    protected HttpClient CreateUserClient(Guid? userId = null) =>
        CreateClient(userId ?? Guid.NewGuid(), UserRoles.User);

    protected HttpClient CreateAdminClient(Guid? userId = null) =>
        CreateClient(userId ?? Guid.NewGuid(), UserRoles.Admin);

    protected static async Task<Guid> CreateBrandProfileAsync(
        HttpClient client,
        string name = "Acme B2B PL",
        BrandBrief? brief = null,
        BrandGenerationDefaults? generationDefaults = null,
        IReadOnlyList<PlatformType>? platforms = null)
    {
        var response = await client.PostAsJsonAsync(
            "/v1/brand-profiles",
            new
            {
                Name = name,
                Brief = brief ?? SampleBrief(),
                Platforms = platforms ?? SamplePlatforms(),
                GenerationDefaults = generationDefaults ?? SampleGenerationDefaults(),
            },
            JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CreateBrandProfileResponse>(JsonOptions);
        return body!.BrandProfileId;
    }

    protected static BrandBrief SampleBrief() =>
        new(
            new BrandCompany("Acme Studio", "Marketing", "Agencja marketingowa"),
            [new BrandOffering("Social Media Management", "Prowadzenie profili firmowych")],
            new BrandTargetAudience("B2B", ["Brak czasu na social media", "Niska liczba leadow"]),
            new BrandIdentity("Professional but friendly", "Expert, accessible, conversational", ["Najlepszy na rynku"]),
            ["Lead Generation", "Brand Awareness"]);

    protected static BrandGenerationDefaults SampleGenerationDefaults() =>
        new("Educational", "Generate leads", "Polish");

    protected static IReadOnlyList<PlatformType> SamplePlatforms() =>
        [PlatformType.LinkedIn];

    protected static async Task AssertErrorCodeAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        response.StatusCode.Should().Be(status);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Errors.Should().Contain(e => e.Code == code);
    }

    protected sealed record CreateBrandProfileResponse(Guid BrandProfileId);

    protected sealed record BrandProfileResponse(
        Guid Id,
        string Name,
        BrandBrief Brief,
        BrandGenerationDefaults? GenerationDefaults,
        List<PlatformType> Platforms,
        List<BrandProfileUserPlatformResponse> UserPlatforms,
        DateTime CreatedAt,
        Guid OwnerId);

    protected sealed record BrandProfileUserPlatformResponse(Guid UserPlatformId, bool IsDefault);

    protected sealed record BrandProfileSummaryResponse(
        Guid Id,
        string Name,
        DateTime CreatedAt,
        Guid OwnerId,
        int LinkedUserPlatformCount);

    protected sealed record PagedResponse<T>(List<T> Items, int Page, int PageSize, long TotalCount);
}

[CollectionDefinition(BrandsCollection.Name)]
public sealed class BrandsCollection : ICollectionFixture<BrandsApp>
{
    public const string Name = "BrandsIntegration";
}
