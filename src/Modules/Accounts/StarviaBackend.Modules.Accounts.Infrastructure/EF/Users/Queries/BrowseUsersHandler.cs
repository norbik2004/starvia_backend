using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Accounts.Application.Users.Queries.BrowseUsers;
using StarviaBackend.Shared.Abstractions.Queries;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Accounts.Infrastructure.EF.Users.Queries;

internal sealed class BrowseUsersHandler(AccountsReadDbContext dbContext)
    : IQueryHandler<BrowseUsersQuery, PagedResult<UserDto>>
{
    public async Task<PagedResult<UserDto>> HandleAsync(
        BrowseUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        var total = await dbContext.Users.LongCountAsync(cancellationToken);
        if (total == 0)
        {
            return PagedResult<UserDto>.Empty(query.Page, query.PageSize);
        }

        var items = await dbContext.Users
            .OrderBy(u => u.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => new UserDto(u.Id, u.Email, u.UserName, u.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<UserDto>(items, query.Page, query.PageSize, total);
    }
}
