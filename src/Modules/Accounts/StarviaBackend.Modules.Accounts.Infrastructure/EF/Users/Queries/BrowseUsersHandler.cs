using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Accounts.Application.Users.Access;
using StarviaBackend.Modules.Accounts.Application.Users.Queries.BrowseUsers;
using StarviaBackend.Modules.Accounts.Core.Users.Access;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Accounts.Infrastructure.EF.Users.Queries;

internal sealed class BrowseUsersHandler(AccountsReadDbContext dbContext, IAccountAccessService access)
    : IQueryHandler<BrowseUsersQuery, PagedResult<UserDto>>
{
    public async Task<PagedResult<UserDto>> HandleAsync(
        BrowseUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        // Admin-only: a non-admin gets 403 here rather than an empty page that looks like "no users".
        var scope = access.RequireScopeFor(AccountAction.BrowseUsers);

        var users = dbContext.Users.AsNoTracking();

        if (!scope.Unrestricted)
        {
            var ownerId = Guid.Parse(scope.OwnerId!);
            users = users.Where(u => u.Id == ownerId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{EscapeLike(query.Search.Trim())}%";
            users = users.Where(u =>
                Microsoft.EntityFrameworkCore.EF.Functions.ILike(u.Email, pattern, "\\")
                || Microsoft.EntityFrameworkCore.EF.Functions.ILike(u.UserName, pattern, "\\"));
        }

        var total = await users.LongCountAsync(cancellationToken);
        var skip = (long)(query.Page - 1) * query.PageSize;
        if (total == 0 || skip >= total)
        {
            return new PagedResult<UserDto>([], query.Page, query.PageSize, total);
        }

        var items = await Order(users, query)
            .Skip((int)skip)
            .Take(query.PageSize)
            .Select(u => new UserDto(u.Id, u.Email, u.UserName, u.CreatedAt, u.LastLoginAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<UserDto>(items, query.Page, query.PageSize, total);
    }

    // Id is the tie-breaker so paging stays stable when the sort key repeats.
    private static IOrderedQueryable<Configurations.Read.Models.UserReadModel> Order(
        IQueryable<Configurations.Read.Models.UserReadModel> users,
        BrowseUsersQuery query) => (query.SortBy, query.Descending) switch
    {
        (UserSortBy.Email, false) => users.OrderBy(u => u.Email).ThenBy(u => u.Id),
        (UserSortBy.Email, true) => users.OrderByDescending(u => u.Email).ThenBy(u => u.Id),
        (UserSortBy.UserName, false) => users.OrderBy(u => u.UserName).ThenBy(u => u.Id),
        (UserSortBy.UserName, true) => users.OrderByDescending(u => u.UserName).ThenBy(u => u.Id),
        (UserSortBy.LastLoginAt, false) => users.OrderBy(u => u.LastLoginAt).ThenBy(u => u.Id),
        (UserSortBy.LastLoginAt, true) => users.OrderByDescending(u => u.LastLoginAt).ThenBy(u => u.Id),
        (_, true) => users.OrderByDescending(u => u.CreatedAt).ThenBy(u => u.Id),
        _ => users.OrderBy(u => u.CreatedAt).ThenBy(u => u.Id),
    };

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}