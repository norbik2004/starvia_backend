using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Platforms.Core.Platforms.Enums;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.Initializers;

internal static class PlatformsSeeder
{
    public static async Task SeedPlatformsAsync(PlatformsWriteDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.Set<Platform>().AnyAsync(cancellationToken: cancellationToken))
        {
            return;
        }

        var facebookPlatform = Platform.Create(PlatformType.Facebook, IsOnline: true);
        var instagramPlatform = Platform.Create(PlatformType.Instagram, IsOnline: false);
        var linkedInPlatform = Platform.Create(PlatformType.LinkedIn, IsOnline: false);

        await dbContext.AddRangeAsync([facebookPlatform, instagramPlatform, linkedInPlatform], cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
