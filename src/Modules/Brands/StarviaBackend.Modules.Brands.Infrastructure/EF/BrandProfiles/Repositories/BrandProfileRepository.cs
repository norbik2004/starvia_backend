using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Entities;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Repositories;
using StarviaBackend.Modules.Brands.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Repositories;

internal sealed class BrandProfileRepository(BrandsWriteDbContext dbContext) : IBrandProfileRepository
{
    public async Task AddAsync(BrandProfile brandProfile, CancellationToken cancellationToken = default)
    {
        await dbContext.BrandProfiles.AddAsync(brandProfile, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<BrandProfile?> GetByIdAsync(Guid brandProfileId, CancellationToken cancellationToken = default) =>
        dbContext.BrandProfiles
            .Include(p => p.UserPlatforms)
            .Include(p => p.Platforms)
            .FirstOrDefaultAsync(p => p.Id == brandProfileId, cancellationToken);

    public async Task UpdateAsync(BrandProfile brandProfile, CancellationToken cancellationToken = default)
    {
        dbContext.BrandProfiles.Update(brandProfile);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(BrandProfile brandProfile, CancellationToken cancellationToken = default)
    {
        dbContext.BrandProfiles.Remove(brandProfile);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearDefaultsForUserPlatformsAsync(
        IEnumerable<Guid> userPlatformIds,
        Guid exceptBrandProfileId,
        CancellationToken cancellationToken = default)
    {
        var ids = userPlatformIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return;
        }

        var links = await dbContext.BrandProfileUserPlatforms
            .Where(l => ids.Contains(l.UserPlatformId) && l.BrandProfileId != exceptBrandProfileId && l.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var link in links)
        {
            link.SetDefault(false);
        }

        if (links.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
