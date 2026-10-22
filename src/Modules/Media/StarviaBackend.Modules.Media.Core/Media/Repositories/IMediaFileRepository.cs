using StarviaBackend.Modules.Media.Core.Media.Entities;

namespace StarviaBackend.Modules.Media.Core.Media.Repositories;

internal interface IMediaFileRepository
{
    Task AddAsync(MediaFile mediaFile, CancellationToken cancellationToken = default);
    Task<MediaFile?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task UpdateAsync(MediaFile mediaFile, CancellationToken cancellationToken = default);
    Task RemoveAsync(MediaFile mediaFile, CancellationToken cancellationToken = default);
}
