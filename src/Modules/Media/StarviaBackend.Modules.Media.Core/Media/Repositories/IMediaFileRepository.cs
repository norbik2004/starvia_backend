using StarviaBackend.Modules.Media.Core.Media.Entities;

namespace StarviaBackend.Modules.Media.Core.Media.Repositories;

internal interface IMediaFileRepository
{
    Task AddAsync(MediaFile mediaFile, CancellationToken cancellationToken = default);
}
