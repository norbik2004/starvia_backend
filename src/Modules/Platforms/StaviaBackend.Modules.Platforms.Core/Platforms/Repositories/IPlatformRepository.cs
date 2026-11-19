using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Platforms.Core.Platforms.Repositories;

internal interface IPlatformRepository
{
    Task<Platform?> GetByIdAsync(Guid platformId, CancellationToken cancellationToken = default);
    Task<Platform?> GetByTypeAsync(PlatformType platformType, CancellationToken cancellationToken = default);
}
