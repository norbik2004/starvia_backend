using StarviaBackend.Modules.Media.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Media.Core.Media.Entities;
using StarviaBackend.Modules.Media.Core.Media.Repositories;
using Microsoft.EntityFrameworkCore;

namespace StarviaBackend.Modules.Media.Infrastructure.EF.MediaFiles.Repositories;

internal sealed class MediaFileRepository(MediaWriteDbContext dbContext) : IMediaFileRepository
{
    public async Task AddAsync(MediaFile mediaFile, CancellationToken cancellationToken = default)
    {
        await dbContext.MediaFiles.AddAsync(mediaFile, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MediaFile?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        return await dbContext.MediaFiles.FirstOrDefaultAsync(mf => mf.Id == fileId, cancellationToken);
    }

    public async Task RemoveAsync(MediaFile mediaFile, CancellationToken cancellationToken = default)
    {
        dbContext.MediaFiles.Remove(mediaFile);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
