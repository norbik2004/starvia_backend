using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;
using StarviaBackend.Modules.Platforms.Core.Platforms.Repositories;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.UserPlatforms.Repositories;

internal class UserPlatformRepository(PlatformsWriteDbContext dbContext) : IUserPlatformRepository
{
    private readonly DbSet<UserPlatform> _userPlatforms = dbContext.UserPlatforms;
    public async Task<UserPlatform?> GetByIdAndUserIdAsync(Guid userPlatformId, Guid UserId, CancellationToken cancellationToken = default)
    {
        return await _userPlatforms.Include(c => c.Platform).FirstOrDefaultAsync(u => userPlatformId == u.Id &&
        u.UserId == UserId, cancellationToken);
    }
}
