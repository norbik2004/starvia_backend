using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Brands.Application.BrandProfiles;
using StarviaBackend.Modules.Brands.Application.BrandProfiles.Access;
using StarviaBackend.Modules.Brands.Application.BrandProfiles.Queries.BrowseBrandProfilesByUserPlatform;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Access;
using StarviaBackend.Modules.Brands.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Queries;

internal sealed class BrowseBrandProfilesByUserPlatformHandler(
    BrandsReadDbContext dbContext,
    IBrandAccessService access)
    : IQueryHandler<BrowseBrandProfilesByUserPlatformQuery, IReadOnlyList<BrandProfileSummaryDto>>
{
    public async Task<IReadOnlyList<BrandProfileSummaryDto>> HandleAsync(
        BrowseBrandProfilesByUserPlatformQuery query,
        CancellationToken cancellationToken = default)
    {
        var scope = access.ScopeFor(BrandAction.Browse);
        if (scope.IsEmpty)
        {
            return [];
        }

        var profiles = dbContext.BrandProfiles
            .AsNoTracking()
            .Where(p => p.UserPlatforms.Any(l => l.UserPlatformId == query.UserPlatformId));

        if (!scope.Unrestricted && Guid.TryParse(scope.OwnerId, out var scopeOwnerId))
        {
            profiles = profiles.Where(p => p.UserId == scopeOwnerId);
        }

        return await profiles
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Select(p => new BrandProfileSummaryDto(
                p.Id,
                p.Name,
                p.CreatedAt,
                p.UserId,
                p.UserPlatforms.Count))
            .ToListAsync(cancellationToken);
    }
}
