using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Brands.Application.BrandProfiles;
using StarviaBackend.Modules.Brands.Application.BrandProfiles.Access;
using StarviaBackend.Modules.Brands.Application.BrandProfiles.Queries.BrowseBrandProfiles;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Access;
using StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Configurations.Read.Models;
using StarviaBackend.Modules.Brands.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Queries;

internal sealed class BrowseBrandProfilesHandler(BrandsReadDbContext dbContext, IBrandAccessService access)
    : IQueryHandler<BrowseBrandProfilesQuery, PagedResult<BrandProfileSummaryDto>>
{
    public async Task<PagedResult<BrandProfileSummaryDto>> HandleAsync(
        BrowseBrandProfilesQuery query,
        CancellationToken cancellationToken = default)
    {
        var scope = access.ScopeFor(BrandAction.Browse);
        if (scope.IsEmpty)
        {
            return PagedResult<BrandProfileSummaryDto>.Empty(query.Page, query.PageSize);
        }

        var profiles = ApplyFilters(dbContext.BrandProfiles.AsNoTracking(), query, scope);

        var total = await profiles.LongCountAsync(cancellationToken);
        var skip = (long)(query.Page - 1) * query.PageSize;
        if (skip >= total)
        {
            return new PagedResult<BrandProfileSummaryDto>([], query.Page, query.PageSize, total);
        }

        var items = await profiles
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Skip((int)skip)
            .Take(query.PageSize)
            .Select(p => new BrandProfileSummaryDto(
                p.Id,
                p.Name,
                p.CreatedAt,
                p.UserId,
                p.UserPlatforms.Count))
            .ToListAsync(cancellationToken);

        return new PagedResult<BrandProfileSummaryDto>(items, query.Page, query.PageSize, total);
    }

    private static IQueryable<BrandProfileReadModel> ApplyFilters(
        IQueryable<BrandProfileReadModel> profiles,
        BrowseBrandProfilesQuery query,
        AccessScope scope)
    {
        if (!scope.Unrestricted && Guid.TryParse(scope.OwnerId, out var scopeOwnerId))
        {
            profiles = profiles.Where(p => p.UserId == scopeOwnerId);
        }

        if (query.OwnerId is { } ownerId)
        {
            profiles = profiles.Where(p => p.UserId == ownerId);
        }

        if (query.UserPlatformId is { } userPlatformId)
        {
            profiles = profiles.Where(p => p.UserPlatforms.Any(l => l.UserPlatformId == userPlatformId));
        }

        return profiles;
    }
}
