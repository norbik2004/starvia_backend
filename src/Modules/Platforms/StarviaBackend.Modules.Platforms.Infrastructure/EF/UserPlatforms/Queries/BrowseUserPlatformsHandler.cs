using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Platforms.Application.Access;
using StarviaBackend.Modules.Platforms.Application.UserPlatforms.Queries.BrowseUserPlatforms;
using StarviaBackend.Modules.Platforms.Core.Platforms.Access;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.UserPlatforms.Configurations.Read.Models;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.UserPlatforms.Queries;

internal sealed class BrowseUserPlatfromsHandler(PlatformsReadDbContext dbContext, IPlatformAccessService access)
    : IQueryHandler<BrowseUserPlatformsQuery, PagedResult<UserPlatformDto>>
{
    public async Task<PagedResult<UserPlatformDto>> HandleAsync(
        BrowseUserPlatformsQuery query,
        CancellationToken cancellationToken = default)
    {
        var scope = access.ScopeFor(PlatformAction.BrowseUserPlatforms);
        if (scope.IsEmpty)
        {
            return PagedResult<UserPlatformDto>.Empty(query.Page, query.PageSize);
        }

        var userPlatforms = ApplyFilters(dbContext.UserPlatforms.AsNoTracking(), query, scope);

        var total = await userPlatforms.LongCountAsync(cancellationToken);

        // Also guards against Skip() overflowing for absurdly large page numbers.
        var skip = (long)(query.Page - 1) * query.PageSize;
        if (skip >= total)
        {
            return new PagedResult<UserPlatformDto>([], query.Page, query.PageSize, total);
        }

        var items = await ApplyOrdering(userPlatforms, query)
            .Skip((int)skip)
            .Take(query.PageSize)
            .Select(u => new UserPlatformDto(u.Id, u.Platform.PlatformType, u.AccountUsername, u.CreatedAt,
                u.ExternalAccountId, u.AccountComment, u.ProfilePictureLink, u.UserId))
            .ToListAsync(cancellationToken);

        return new PagedResult<UserPlatformDto>(items, query.Page, query.PageSize, total);
    }

    private static IQueryable<UserPlatformReadModel> ApplyFilters(
        IQueryable<UserPlatformReadModel> userPlatforms,
        BrowseUserPlatformsQuery query,
        AccessScope scope)
    {
        if (!scope.Unrestricted && Guid.TryParse(scope.OwnerId, out var scopeOwnerId))
        {
            userPlatforms = userPlatforms.Where(u => u.UserId == scopeOwnerId);
        }

        if (query.OwnerId is { } ownerId)
        {
            userPlatforms = userPlatforms.Where(u => u.UserId == ownerId);
        }

        if (query.PlatformType is { } platformType)
        {
            userPlatforms = userPlatforms.Where(u => u.Platform.PlatformType == platformType);
        }

        if (query.Search is { } search)
        {
            var pattern = $"%{EscapeLikePattern(search)}%";
            userPlatforms = userPlatforms.Where(u =>
                Microsoft.EntityFrameworkCore.EF.Functions.ILike(u.AccountUsername, pattern, "\\"));
        }

        return userPlatforms;
    }

    // Id is the tie-breaker so paging stays stable when the sort column has duplicates.
    private static IQueryable<UserPlatformReadModel> ApplyOrdering(
        IQueryable<UserPlatformReadModel> userPlatforms,
        BrowseUserPlatformsQuery query) =>
        (query.SortBy, query.Descending) switch
        {
            (UserPlatformSortBy.AccountUsername, true) => userPlatforms.OrderByDescending(u => u.AccountUsername).ThenByDescending(u => u.Id),
            (UserPlatformSortBy.AccountUsername, false) => userPlatforms.OrderBy(u => u.AccountUsername).ThenBy(u => u.Id),
            (_, true) => userPlatforms.OrderByDescending(u => u.CreatedAt).ThenByDescending(u => u.Id),
            (_, false) => userPlatforms.OrderBy(u => u.CreatedAt).ThenBy(u => u.Id),
        };

    private static string EscapeLikePattern(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
