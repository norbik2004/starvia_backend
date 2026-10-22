using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Initializers;
using StarviaBackend.Shared.Tests.Integration;

namespace StarviaBackend.Modules.Platforms.Tests.Integration;

/// <summary>Test host for the Platforms module: applies its migrations and seeds the platforms catalogue.</summary>
public sealed class PlatformsApp : BoilerplateApp
{
    protected override async Task RunMigrationsAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PlatformsWriteDbContext>();
        await dbContext.Database.MigrateAsync();
        await PlatformsSeeder.SeedPlatformsAsync(dbContext, CancellationToken.None);
    }
}
