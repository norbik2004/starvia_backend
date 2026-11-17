using StarviaBackend.Modules.Posts.Core.Posts.Enums;
using StarviaBackend.Shared.Abstractions.Platforms;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Posts.Application.Posts.Queries.BrowsePosts;

/// <summary>
/// Lists the posts the caller is allowed to browse (see the posts access policy): everything for
/// admins, own posts for everybody else. The filters below narrow that set further.
/// </summary>
/// <param name="Status">Only posts in this status.</param>
/// <param name="Platforms">
/// Comma-separated platform names or ints (e.g. <c>Facebook,Instagram,LinkedIn</c>).
/// Only posts that include any of these platforms in <c>Platforms</c>.
/// </param>
/// <param name="Search">Case-insensitive text the title must contain.</param>
/// <param name="CreatedBy">Only posts created by this user (mostly useful for admins).</param>
/// <param name="SortBy">Sort column, newest first by default.</param>
/// <param name="Descending">Sort direction.</param>
internal sealed record BrowsePostsQuery(
    int Page = 1,
    int PageSize = 20,
    PostStatus? Status = null,
    string? Platforms = null,
    string? Search = null,
    Guid? CreatedBy = null,
    PostSortBy SortBy = PostSortBy.CreatedAt,
    bool Descending = true) : IPagedQuery<PostDto>
{
    public const int MaxSearchLength = 100;

    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize is < 1 or > 100 ? 20 : PageSize;

    /// <summary>Parsed from the <c>platforms</c> query string; null when empty / invalid.</summary>
    public IReadOnlyList<PlatformType>? PlatformTypes => ParsePlatforms(Platforms);

    public string? Search { get; init; } = NormalizeSearch(Search);

    private static IReadOnlyList<PlatformType>? ParsePlatforms(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var parsed = new List<PlatformType>();
        foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (Enum.TryParse(part, ignoreCase: true, out PlatformType platform) &&
                Enum.IsDefined(platform))
            {
                parsed.Add(platform);
                continue;
            }

            if (int.TryParse(part, out var number) &&
                Enum.IsDefined(typeof(PlatformType), number))
            {
                parsed.Add((PlatformType)number);
            }
        }

        return parsed.Count > 0 ? parsed.Distinct().ToList() : null;
    }

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
    IReadOnlyList<PlatformType> Platforms,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? LastModifiedAt,
    int AttachmentsCount);
