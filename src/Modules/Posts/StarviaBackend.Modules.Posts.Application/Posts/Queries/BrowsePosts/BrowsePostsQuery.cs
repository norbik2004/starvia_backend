using StarviaBackend.Modules.Posts.Core.Posts.Enums;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Posts.Application.Posts.Queries.BrowsePosts;

/// <summary>
/// Lists the posts the caller is allowed to browse (see the posts access policy): everything for
/// admins, own posts for everybody else. The filters below narrow that set further.
/// </summary>
/// <param name="Status">Only posts in this status.</param>
/// <param name="Search">Case-insensitive text the title must contain.</param>
/// <param name="CreatedBy">Only posts created by this user (mostly useful for admins).</param>
/// <param name="SortBy">Sort column, newest first by default.</param>
/// <param name="Descending">Sort direction.</param>
internal sealed record BrowsePostsQuery(
    int Page = 1,
    int PageSize = 20,
    PostStatus? Status = null,
    string? Search = null,
    Guid? CreatedBy = null,
    PostSortBy SortBy = PostSortBy.CreatedAt,
    bool Descending = true) : IPagedQuery<PostDto>
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

public enum PostSortBy
{
    CreatedAt,
    LastModifiedAt,
    Title,
}

public sealed record PostDto(
    Guid Id,
    string Title,
    PostStatus Status,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? LastModifiedAt,
    int AttachmentsCount);
