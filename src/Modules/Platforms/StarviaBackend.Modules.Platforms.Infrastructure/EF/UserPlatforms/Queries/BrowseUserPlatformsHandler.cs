using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Platforms.Application.UserPlatforms.Queries.BrowseUserPlatforms;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.UserPlatforms.Queries;

internal sealed class BrowseUserPlatfromsHandler(PlatformsReadDbContext dbContext)
    : IQueryHandler<BrowseUserPlatformsQuery, PagedResult<UserPlatformDto>>
{
    public async Task<PagedResult<UserPlatformDto>> HandleAsync(
        BrowseUserPlatformsQuery query,
        CancellationToken cancellationToken = default)
    {
        var total = await dbContext.Platforms.LongCountAsync(cancellationToken);
        if (total == 0)
        {
            return PagedResult<UserPlatformDto>.Empty(query.Page, query.PageSize);
        }

        var items = await dbContext.UserPlatforms
            .OrderBy(u => u.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => new UserPlatformDto(u.Id, u.Platform.PlatformType, u.AccountUsername, u.CreatedAt,
            u.ExternalAccountId, u.AccountComment, u.ProfilePictureLink))
            .ToListAsync(cancellationToken);

        return new PagedResult<UserPlatformDto>(items, query.Page, query.PageSize, total);
    }
}
