using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Time;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.Initializers;

internal sealed class PlatformsDataInitializer(IServiceProvider serviceProvider, ILogger<PlatformsDataInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PlatformsWriteDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);

        logger.LogInformation("Seeding platforms");
        await PlatformsSeeder.SeedPlatformsAsync(dbContext, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
