using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Media.Application.MediaFiles.Queries.DownloadFile;
using StarviaBackend.Modules.Media.Core.Media.Exceptions;
using StarviaBackend.Modules.Media.Core.Media.Storage;
using StarviaBackend.Modules.Media.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;
using StarviaBackend.Shared.Abstractions.Storage;

namespace StarviaBackend.Modules.Media.Infrastructure.EF.MediaFiles.Queries;

internal sealed class DownloadFileHandler(MediaReadDbContext dbContext, IFileStorageService fileStorage)
    : IQueryHandler<DownloadFileQuery, FileContentDto>
{
    public async Task<FileContentDto> HandleAsync(DownloadFileQuery query, CancellationToken cancellationToken = default)
    {
        var file = await dbContext.MediaFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == query.FileId, cancellationToken)
            ?? throw new MediaNotFoundException(query.FileId);

        if (file.CreatedBy != query.UserId.ToString())
        {
            throw new UnauthorizedAccessException("You cannot download this file.");
        }

        var content = await fileStorage.DownloadAsync(file.FilePath, BucketNames.UserUploads, cancellationToken);

        return new FileContentDto(content, file.FileName);
    }
}
