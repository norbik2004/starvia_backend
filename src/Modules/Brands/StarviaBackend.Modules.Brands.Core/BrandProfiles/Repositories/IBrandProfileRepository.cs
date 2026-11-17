using StarviaBackend.Modules.Brands.Core.BrandProfiles.Entities;

namespace StarviaBackend.Modules.Brands.Core.BrandProfiles.Repositories;

internal interface IBrandProfileRepository
{
    Task AddAsync(BrandProfile brandProfile, CancellationToken cancellationToken = default);
    Task<BrandProfile?> GetByIdAsync(Guid brandProfileId, CancellationToken cancellationToken = default);
    Task UpdateAsync(BrandProfile brandProfile, CancellationToken cancellationToken = default);
    Task RemoveAsync(BrandProfile brandProfile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears <c>IsDefault</c> on every link for the given user platforms except those belonging to
    /// <paramref name="exceptBrandProfileId"/>.
    /// </summary>
    Task ClearDefaultsForUserPlatformsAsync(
        IEnumerable<Guid> userPlatformIds,
        Guid exceptBrandProfileId,
        CancellationToken cancellationToken = default);
}
