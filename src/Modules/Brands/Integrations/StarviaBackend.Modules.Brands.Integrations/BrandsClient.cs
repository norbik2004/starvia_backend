using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using StarviaBackend.Modules.Brands.Integrations.External;
using StarviaBackend.Modules.Brands.Integrations.Models;
using StarviaBackend.Shared.Infrastructure.Integrations;
using StarviaBackend.Shared.Infrastructure.Serialization;

namespace StarviaBackend.Modules.Brands.Integrations;

internal sealed class BrandsClient(HttpClient httpClient) : IBrandsClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new PlatformTypeJsonConverter() },
    };

    public async Task<BrandProfileSummary> GetProfileAsync(
        Guid userId,
        Guid brandProfileId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new BrandsIntegrationException("User id cannot be empty.");
        }

        if (brandProfileId == Guid.Empty)
        {
            throw new BrandsIntegrationException("Brand profile id cannot be empty.");
        }

        using var response = await SendAsync($"v1/brand-profiles/{brandProfileId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await response.ToFailureAsync(FailureRules(brandProfileId), cancellationToken);
        }

        var payload = await response.Content.ReadFromJsonAsync<BrandProfileApiResponse>(JsonOptions, cancellationToken)
                      ?? throw new BrandsIntegrationException("Brands API returned an empty body.");

        if (payload.OwnerId != userId)
        {
            throw new BrandsIntegrationNotOwnerException("User is not the owner of brand profile.");
        }

        return Map(payload);
    }

    private static BrandProfileSummary Map(BrandProfileApiResponse payload) =>
        new(
            payload.Id,
            payload.Name,
            new BrandBrief(
                payload.Brief.Company is null
                    ? null
                    : new BrandCompany(
                        payload.Brief.Company.Name,
                        payload.Brief.Company.Industry,
                        payload.Brief.Company.Description),
                payload.Brief.Offerings?
                    .Select(o => new BrandOffering(o.Name, o.Description))
                    .ToList(),
                payload.Brief.TargetAudience is null
                    ? null
                    : new BrandTargetAudience(
                        payload.Brief.TargetAudience.Type,
                        payload.Brief.TargetAudience.PainPoints),
                payload.Brief.BrandIdentity is null
                    ? null
                    : new BrandIdentity(
                        payload.Brief.BrandIdentity.ToneOfVoice,
                        payload.Brief.BrandIdentity.Style,
                        payload.Brief.BrandIdentity.WordsToAvoid),
                payload.Brief.MarketingGoals),
            payload.GenerationDefaults is null
                ? null
                : new BrandGenerationDefaults(
                    payload.GenerationDefaults.PostType,
                    payload.GenerationDefaults.Goal,
                    payload.GenerationDefaults.Language),
            payload.Platforms ?? [],
            payload.OwnerId);

    private static IntegrationFailureRules FailureRules(Guid brandProfileId) =>
        new IntegrationFailureRules(status => new BrandsIntegrationException($"Brands API returned {(int)status}."))
            .OnStatus(HttpStatusCode.NotFound, () => ProfileNotFound(brandProfileId))
            .OnCode("brand_profile_not_found", () => ProfileNotFound(brandProfileId))
            .OnCode("brand_profile_access_denied", () =>
                new BrandsIntegrationNotOwnerException("User is not the owner of brand profile."));

    private static BrandsIntegrationException ProfileNotFound(Guid brandProfileId) =>
        new($"Brand profile '{brandProfileId}' was not found in the brands API.");

    private async Task<HttpResponseMessage> SendAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.GetAsync(path, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException
                                   || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            throw new BrandsIntegrationException($"Brands API unreachable: {ex.Message}");
        }
    }
}
