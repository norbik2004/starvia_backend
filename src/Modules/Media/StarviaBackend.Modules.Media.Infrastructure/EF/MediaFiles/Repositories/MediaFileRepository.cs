using StarviaBackend.Modules.Media.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Media.Core.Media.Entities;
using StarviaBackend.Modules.Media.Core.Media.Repositories;

namespace StarviaBackend.Modules.Media.Infrastructure.EF.MediaFiles.Repositories;

internal sealed class MediaFileRepository(MediaWriteDbContext dbContext) : IMediaFileRepository
{
    public async Task AddAsync(MediaFile mediaFile, CancellationToken cancellationToken = default)
    {
        await dbContext.MediaFiles.AddAsync(mediaFile, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
