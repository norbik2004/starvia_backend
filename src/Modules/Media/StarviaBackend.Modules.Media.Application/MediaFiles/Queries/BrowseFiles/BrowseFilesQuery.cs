using StarviaBackend.Modules.Media.Core.Media.Enums;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Media.Application.MediaFiles.Queries.BrowseFiles;

internal sealed record BrowseFilesQuery(Guid UserId, int Page = 1, int PageSize = 20) : IPagedQuery<FileDto>
{
    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize is < 1 or > 100 ? 20 : PageSize;
}

public sealed record BrowseFilesRequest(int Page = 1, int PageSize = 20);

public sealed record FileDto(Guid Id, string FileName, string? Description, FileSource Source, FileType Type,
    string Url, DateTime CreatedAt);
