using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Media.Application.MediaFiles.Access;
using StarviaBackend.Modules.Media.Application.MediaFiles.Queries.DownloadFile;
using StarviaBackend.Modules.Media.Core.Media.Access;
using StarviaBackend.Modules.Media.Core.Media.Exceptions;
using StarviaBackend.Modules.Media.Core.Media.Storage;
using StarviaBackend.Modules.Media.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;
using StarviaBackend.Shared.Abstractions.Storage;

namespace StarviaBackend.Modules.Media.Infrastructure.EF.MediaFiles.Queries;

internal sealed class DownloadFileHandler(
    MediaReadDbContext dbContext,
    IFileStorageService fileStorage,
    IMediaAccessService access)
    : IQueryHandler<DownloadFileQuery, FileContentDto>
{
    public async Task<FileContentDto> HandleAsync(DownloadFileQuery query, CancellationToken cancellationToken = default)
    {
        var file = await dbContext.MediaFiles
            .AsNoTracking()
            .Where(f => f.Id == query.FileId)
            .Select(f => new { f.FilePath, f.FileName, f.CreatedBy })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new MediaNotFoundException(query.FileId);

        access.EnsureAllowed(MediaAction.Download, query.FileId, file.CreatedBy);

        var content = await fileStorage.DownloadAsync(file.FilePath, BucketNames.UserUploads, cancellationToken);

        return new FileContentDto(content, file.FileName);
    }
}
