using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Media.Application.MediaFiles;
using StarviaBackend.Modules.Media.Application.MediaFiles.Access;
using StarviaBackend.Modules.Media.Application.MediaFiles.Queries.BrowseFiles;
using StarviaBackend.Modules.Media.Core.Media.Access;
using StarviaBackend.Modules.Media.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Media.Infrastructure.EF.MediaFiles.Configurations.Read.Models;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Media.Infrastructure.EF.MediaFiles.Queries;

internal sealed class BrowseFilesHandler(MediaReadDbContext dbContext, IMediaAccessService access)
    : IQueryHandler<BrowseFilesQuery, PagedResult<FileDto>>
{
    public async Task<PagedResult<FileDto>> HandleAsync(BrowseFilesQuery query, CancellationToken cancellationToken = default)
    {
        var scope = access.ScopeFor(MediaAction.Browse);
        if (scope.IsEmpty)
        {
            return PagedResult<FileDto>.Empty(query.Page, query.PageSize);
        }

        var files = ApplyFilters(dbContext.MediaFiles.AsNoTracking(), query, scope);

        var total = await files.LongCountAsync(cancellationToken);

        // Also guards against Skip() overflowing for absurdly large page numbers.
        var skip = (long)(query.Page - 1) * query.PageSize;
        if (skip >= total)
        {
            return new PagedResult<FileDto>([], query.Page, query.PageSize, total);
        }

        var rows = await ApplyOrdering(files, query)
            .Skip((int)skip)
            .Take(query.PageSize)
            .Select(f => new { f.Id, f.FileName, f.Description, f.Source, f.Type, f.CreatedAt, f.CreatedBy })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(f => new FileDto(
                f.Id,
                f.FileName,
                f.Description,
                f.Source,
                f.Type,
                MediaFileRoutes.Content(f.Id),
                f.CreatedAt,
                f.CreatedBy))
            .ToList();

        return new PagedResult<FileDto>(items, query.Page, query.PageSize, total);
    }

    private static IQueryable<MediaFileReadModel> ApplyFilters(
        IQueryable<MediaFileReadModel> files,
        BrowseFilesQuery query,
        AccessScope scope)
    {
        if (!scope.Unrestricted)
        {
            var ownerId = scope.OwnerId;
            files = files.Where(f => f.CreatedBy == ownerId);
        }

        if (query.CreatedBy is { } createdBy)
        {
            var createdById = createdBy.ToString();
            files = files.Where(f => f.CreatedBy == createdById);
        }

        if (query.Type is { } type)
        {
            files = files.Where(f => f.Type == type);
        }

        if (query.Source is { } source)
        {
            files = files.Where(f => f.Source == source);
        }

        if (query.Search is { } search)
        {
            var pattern = $"%{EscapeLikePattern(search)}%";
            files = files.Where(f =>
                Microsoft.EntityFrameworkCore.EF.Functions.ILike(f.FileName, pattern, "\\")
                || Microsoft.EntityFrameworkCore.EF.Functions.ILike(f.Description!, pattern, "\\"));
        }

        return files;
    }

    // Id is the tie-breaker so paging stays stable when the sort column has duplicates.
    private static IQueryable<MediaFileReadModel> ApplyOrdering(IQueryable<MediaFileReadModel> files, BrowseFilesQuery query) =>
        (query.SortBy, query.Descending) switch
        {
            (FileSortBy.FileName, true) => files.OrderByDescending(f => f.FileName).ThenByDescending(f => f.Id),
            (FileSortBy.FileName, false) => files.OrderBy(f => f.FileName).ThenBy(f => f.Id),
            (_, true) => files.OrderByDescending(f => f.CreatedAt).ThenByDescending(f => f.Id),
            (_, false) => files.OrderBy(f => f.CreatedAt).ThenBy(f => f.Id),
        };

    private static string EscapeLikePattern(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
