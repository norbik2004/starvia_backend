using Microsoft.EntityFrameworkCore;
using StarviaBackend.Shared.Abstractions.Queries;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Platforms.Application.Platforms.Queries.BrowsePlatforms;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.Platforms.Queries;

internal sealed class BrowsePlatfromsHandler(PlatformsReadDbContext dbContext)
    : IQueryHandler<BrowsePlatformsQuery, PagedResult<PlatformDto>>
{
    public async Task<PagedResult<PlatformDto>> HandleAsync(
        BrowsePlatformsQuery query,
        CancellationToken cancellationToken = default)
    {
        var total = await dbContext.Platforms.LongCountAsync(cancellationToken);
        if (total == 0)
        {
            return PagedResult<PlatformDto>.Empty(query.Page, query.PageSize);
        }

        var items = await dbContext.Platforms
            .OrderBy(u => u.PlatformType)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => new PlatformDto(u.Id, u.PlatformType, u.IsOnline))
            .ToListAsync(cancellationToken);

        return new PagedResult<PlatformDto>(items, query.Page, query.PageSize, total);
    }
}
