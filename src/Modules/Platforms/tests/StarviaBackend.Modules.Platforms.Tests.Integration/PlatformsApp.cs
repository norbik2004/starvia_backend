using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StarviaBackend.Modules.Media.Integrations;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Initializers;
using StarviaBackend.Modules.Platforms.Integrations.LinkedIn;
using StarviaBackend.Modules.Posts.Integrations;
using StarviaBackend.Shared.Tests.Integration;

namespace StarviaBackend.Modules.Platforms.Tests.Integration;

/// <summary>Test host for the Platforms module: applies its migrations and seeds the platforms catalogue.</summary>
public sealed class PlatformsApp : BoilerplateApp
{
    public FakePostsClient Posts { get; } = new();
    public FakeMediaClient Media { get; } = new();
    public FakeLinkedInClient LinkedIn { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IPostsClient>();
            services.AddSingleton<IPostsClient>(Posts);

            services.RemoveAll<IMediaClient>();
            services.AddSingleton<IMediaClient>(Media);

            services.RemoveAll<ILinkedInClient>();
            services.AddSingleton<ILinkedInClient>(LinkedIn);
        });
    }

    protected override async Task RunMigrationsAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PlatformsWriteDbContext>();
        await dbContext.Database.MigrateAsync();
        await PlatformsSeeder.SeedPlatformsAsync(dbContext, CancellationToken.None);
    }
}
