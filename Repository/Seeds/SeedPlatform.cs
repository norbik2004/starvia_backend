using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Core.Enums;

namespace Repository.Seeds
{
    public static class SeedPlatforms
    {
        public async static Task Seed(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<TrDbContext>();

            var existingPlatforms = await dbContext.Platforms.ToListAsync();

            if (!existingPlatforms.Any(x => x.Type == PlatformType.LinkedIn))
            {
                await dbContext.Platforms.AddAsync(new Platform
                {
                    Type = PlatformType.LinkedIn
                });
            }

            if (!existingPlatforms.Any(x => x.Type == PlatformType.Facebook))
            {
                await dbContext.Platforms.AddAsync(new Platform
                {
                    Type = PlatformType.Facebook
                });
            }

            if (!existingPlatforms.Any(x => x.Type == PlatformType.Instagram))
            {
                await dbContext.Platforms.AddAsync(new Platform
                {
                    Type = PlatformType.Instagram
                });
            }

            await dbContext.SaveChangesAsync();
        }
    }
}