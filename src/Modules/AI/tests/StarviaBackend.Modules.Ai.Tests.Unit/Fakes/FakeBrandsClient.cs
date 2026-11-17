using StarviaBackend.Modules.Brands.Integrations;
using StarviaBackend.Modules.Brands.Integrations.Models;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Ai.Tests.Unit.Fakes;

internal sealed class FakeBrandsClient : IBrandsClient
{
    public HashSet<Guid> UnknownProfileIds { get; } = [];
    public HashSet<Guid> ForeignProfileIds { get; } = [];
    public Dictionary<Guid, BrandProfileSummary> Profiles { get; } = new();

    public List<(Guid UserId, Guid BrandProfileId)> Calls { get; } = [];

    public Task<BrandProfileSummary> GetProfileAsync(
        Guid userId,
        Guid brandProfileId,
        CancellationToken cancellationToken = default)
    {
        Calls.Add((userId, brandProfileId));

        if (UnknownProfileIds.Contains(brandProfileId))
        {
            throw new BrandsIntegrationException($"Brand profile '{brandProfileId}' was not found in the brands API.");
        }

        if (ForeignProfileIds.Contains(brandProfileId))
        {
            throw new BrandsIntegrationNotOwnerException("User is not the owner of brand profile.");
        }

        if (Profiles.TryGetValue(brandProfileId, out var profile))
        {
            return Task.FromResult(profile);
        }

        return Task.FromResult(new BrandProfileSummary(
            brandProfileId,
            "Acme",
            new BrandBrief(
                new BrandCompany("Acme Studio", "Marketing", "Agencja"),
                null,
                null,
                null,
                null),
            new BrandGenerationDefaults("Educational", "Generate leads", "Polish"),
            [PlatformType.LinkedIn],
            userId));
    }
}
