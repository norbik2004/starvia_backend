using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;
using StarviaBackend.Modules.Platforms.Core.Platforms.Repositories;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.Platforms.Repositories;

internal class PlatformRepository(PlatformsWriteDbContext dbContext) : IPlatformRepository
{
    private readonly DbSet<Platform> _platforms = dbContext.Platforms;
    public async Task<Platform?> GetByIdAsync(Guid platformId, CancellationToken cancellationToken = default)
    {
        return await _platforms.FirstOrDefaultAsync(c => c.Id == platformId, cancellationToken: cancellationToken);
    }
}
