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

    public async Task AddAsync(UserPlatform userPlatform)
    {
        await _userPlatforms.AddAsync(userPlatform);
        await dbContext.SaveChangesAsync();
    }
}
