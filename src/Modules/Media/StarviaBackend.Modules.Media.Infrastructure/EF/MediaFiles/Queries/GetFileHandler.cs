using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Media.Application.MediaFiles;
using StarviaBackend.Modules.Media.Application.MediaFiles.Queries.GetFile;
using StarviaBackend.Modules.Media.Core.Media.Exceptions;
using StarviaBackend.Modules.Media.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Media.Infrastructure.EF.MediaFiles.Queries;

internal sealed class GetFileHandler(MediaReadDbContext dbContext)
    : IQueryHandler<GetFileQuery, FileLongDto>
{
    public async Task<FileLongDto> HandleAsync(GetFileQuery query, CancellationToken cancellationToken = default)
    {
        var file = await dbContext.MediaFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == query.FileId, cancellationToken)
            ?? throw new MediaNotFoundException(query.FileId);

        if (file.CreatedBy != query.UserId.ToString())
        {
            throw new UnauthorizedAccessException("You cannot see this file.");
        }

        return new FileLongDto(file.Id, file.FileName, file.Description, file.Source, file.Type,
            MediaFileRoutes.Content(file.Id), file.CreatedAt);
    }
}
