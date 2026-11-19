using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;
using StarviaBackend.Modules.Platforms.Core.Platforms.Repositories;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.Platforms.Repositories;

internal sealed class PlatformRepository(PlatformsWriteDbContext dbContext) : IPlatformRepository
{
    private readonly DbSet<Platform> _platforms = dbContext.Platforms;

    public async Task<Platform?> GetByIdAsync(Guid platformId, CancellationToken cancellationToken = default)
    {
        return await _platforms.FirstOrDefaultAsync(c => c.Id == platformId, cancellationToken: cancellationToken);
    }

    public async Task<Platform?> GetByTypeAsync(PlatformType platformType, CancellationToken cancellationToken = default)
    {
        return await _platforms.FirstOrDefaultAsync(c => c.PlatformType == platformType, cancellationToken: cancellationToken);
    }
}
