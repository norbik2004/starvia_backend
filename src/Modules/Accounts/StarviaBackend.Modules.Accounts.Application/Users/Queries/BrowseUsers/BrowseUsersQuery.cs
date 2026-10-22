using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Accounts.Application.Users.Queries.BrowseUsers;

public sealed record BrowseUsersQuery(int Page = 1, int PageSize = 20) : IPagedQuery<UserDto>
{
    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize is < 1 or > 100 ? 20 : PageSize;
}

public sealed record UserDto(Guid Id, string Email, string UserName, DateTime CreatedAt);
