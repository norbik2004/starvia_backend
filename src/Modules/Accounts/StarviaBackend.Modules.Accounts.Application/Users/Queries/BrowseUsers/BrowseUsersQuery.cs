using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Accounts.Application.Users.Queries.BrowseUsers;

public enum UserSortBy
{
    CreatedAt,
    Email,
    UserName,
    LastLoginAt,
}

/// <summary>
/// Lists accounts (admin only - the access policy decides). <paramref name="Search"/> matches the e-mail
/// or the user name, case-insensitively, with wildcards treated literally.
/// </summary>
public sealed record BrowseUsersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    UserSortBy SortBy = UserSortBy.CreatedAt,
    bool Descending = false) : IPagedQuery<UserDto>
{
    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize is < 1 or > 100 ? 20 : PageSize;
}

public sealed record UserDto(Guid Id, string Email, string UserName, DateTime CreatedAt, DateTime? LastLoginAt = null);