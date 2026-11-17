using StarviaBackend.Modules.Brands.Integrations.Models;

namespace StarviaBackend.Modules.Brands.Integrations;

/// <summary>
/// The port other modules depend on. Callers speak in these terms and never see the brands API's DTOs.
/// </summary>
public interface IBrandsClient
{
    /// <summary>
    /// Loads a brand profile on behalf of <paramref name="userId"/>.
    /// Throws when the profile does not exist or the user may not view it.
    /// </summary>
    Task<BrandProfileSummary> GetProfileAsync(
        Guid userId,
        Guid brandProfileId,
        CancellationToken cancellationToken = default);
}
