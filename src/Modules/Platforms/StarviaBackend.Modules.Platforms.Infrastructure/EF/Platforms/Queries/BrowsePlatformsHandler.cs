using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Platforms.Application.Access;
using StarviaBackend.Modules.Platforms.Application.Platforms.Queries.BrowsePlatforms;
using StarviaBackend.Modules.Platforms.Core.Platforms.Access;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.Platforms.Queries;

internal sealed class BrowsePlatfromsHandler(PlatformsReadDbContext dbContext, IPlatformAccessService access)
    : IQueryHandler<BrowsePlatformsQuery, PagedResult<PlatformDto>>
{
    public async Task<PagedResult<PlatformDto>> HandleAsync(
        BrowsePlatformsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (access.ScopeFor(PlatformAction.BrowseCatalog).IsEmpty)
        {
            return PagedResult<PlatformDto>.Empty(query.Page, query.PageSize);
        }

        var platforms = dbContext.Platforms.AsNoTracking();

        if (query.PlatformType is { } platformType)
        {
            platforms = platforms.Where(p => p.PlatformType == platformType);
        }

        if (query.IsOnline is { } isOnline)
        {
            platforms = platforms.Where(p => p.IsOnline == isOnline);
        }

        var total = await platforms.LongCountAsync(cancellationToken);

        // Also guards against Skip() overflowing for absurdly large page numbers.
        var skip = (long)(query.Page - 1) * query.PageSize;
        if (skip >= total)
        {
            return new PagedResult<PlatformDto>([], query.Page, query.PageSize, total);
        }

        var items = await platforms
            .OrderBy(p => p.PlatformType)
            .ThenBy(p => p.Id)
            .Skip((int)skip)
            .Take(query.PageSize)
            .Select(p => new PlatformDto(p.Id, p.PlatformType, p.IsOnline))
            .ToListAsync(cancellationToken);

        return new PagedResult<PlatformDto>(items, query.Page, query.PageSize, total);
    }
}
