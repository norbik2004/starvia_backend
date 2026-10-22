using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Media.Application.MediaFiles;
using StarviaBackend.Modules.Media.Application.MediaFiles.Queries.BrowseFiles;
using StarviaBackend.Modules.Media.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Media.Infrastructure.EF.MediaFiles.Queries;

internal sealed class BrowseFilesHandler(MediaReadDbContext dbContext)
    : IQueryHandler<BrowseFilesQuery, PagedResult<FileDto>>
{
    public async Task<PagedResult<FileDto>> HandleAsync(BrowseFilesQuery query, CancellationToken cancellationToken = default)
    {
        var userId = query.UserId.ToString();

        var files = dbContext.MediaFiles
            .AsNoTracking()
            .Where(f => f.CreatedBy == userId);

        var total = await files.LongCountAsync(cancellationToken);

        if (total == 0)
        {
            return PagedResult<FileDto>.Empty(query.Page, query.PageSize);
        }

        var rows = await files
            .OrderByDescending(f => f.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(f => new { f.Id, f.FileName, f.Description, f.Source, f.Type, f.CreatedAt })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(f => new FileDto(
                f.Id,
                f.FileName,
                f.Description,
                f.Source,
                f.Type,
                MediaFileRoutes.Content(f.Id),
                f.CreatedAt))
            .ToList();

        return new PagedResult<FileDto>(items, query.Page, query.PageSize, total);
    }
}
