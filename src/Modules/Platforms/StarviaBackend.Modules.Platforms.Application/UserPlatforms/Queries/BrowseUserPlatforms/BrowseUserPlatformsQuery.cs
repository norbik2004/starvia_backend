using StarviaBackend.Modules.Platforms.Core.Platforms.Enums;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Platforms.Application.UserPlatforms.Queries.BrowseUserPlatforms;

/// <summary>
/// Lists the connected accounts the caller is allowed to browse (see the platforms access policy):
/// everything for admins, own accounts for everybody else. The filters below narrow that set further.
/// </summary>
/// <param name="PlatformType">Only accounts on this platform.</param>
/// <param name="Search">Case-insensitive text the account name must contain.</param>
/// <param name="OwnerId">Only accounts connected by this user (mostly useful for admins).</param>
/// <param name="SortBy">Sort column, oldest first by default.</param>
/// <param name="Descending">Sort direction.</param>
public sealed record BrowseUserPlatformsQuery(
    int Page = 1,
    int PageSize = 20,
    PlatformType? PlatformType = null,
    string? Search = null,
    Guid? OwnerId = null,
    UserPlatformSortBy SortBy = UserPlatformSortBy.CreatedAt,
    bool Descending = false) : IPagedQuery<UserPlatformDto>
{
    public const int MaxSearchLength = 100;

    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize is < 1 or > 100 ? 20 : PageSize;

    public string? Search { get; init; } = NormalizeSearch(Search);

    private static string? NormalizeSearch(string? search)
    {
        var trimmed = search?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        return trimmed.Length > MaxSearchLength ? trimmed[..MaxSearchLength] : trimmed;
    }
}

public enum UserPlatformSortBy
{
    CreatedAt,
    AccountUsername,
}

public sealed record UserPlatformDto(Guid Id, PlatformType PlatformType, string AccountUsername,
    DateTime CreatedAt, string? ExternalAccountId, string? AccountComment, string? ProfilePictureLink, Guid OwnerId);
