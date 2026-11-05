using StarviaBackend.Modules.Media.Core.Media.Enums;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Media.Application.MediaFiles.Queries.BrowseFiles;

/// <summary>
/// Lists the files the caller is allowed to browse (see the media access policy): everything for
/// admins, own files for everybody else. The filters below narrow that set further.
/// </summary>
/// <param name="Type">Only files of this type.</param>
/// <param name="Source">Only files from this source.</param>
/// <param name="Search">Case-insensitive text the file name or description must contain.</param>
/// <param name="CreatedBy">Only files uploaded by this user (mostly useful for admins).</param>
/// <param name="SortBy">Sort column, newest first by default.</param>
/// <param name="Descending">Sort direction.</param>
internal sealed record BrowseFilesQuery(
    int Page = 1,
    int PageSize = 20,
    FileType? Type = null,
    FileSource? Source = null,
    string? Search = null,
    Guid? CreatedBy = null,
    FileSortBy SortBy = FileSortBy.CreatedAt,
    bool Descending = true) : IPagedQuery<FileDto>
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

public enum FileSortBy
{
    CreatedAt,
    FileName,
}

public sealed record FileDto(Guid Id, string FileName, string? Description, FileSource Source, FileType Type,
    string Url, DateTime CreatedAt, string? CreatedBy);
