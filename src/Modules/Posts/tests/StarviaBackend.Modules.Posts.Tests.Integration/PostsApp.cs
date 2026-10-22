using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StarviaBackend.Modules.Media.Integrations;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Tests.Integration;

namespace StarviaBackend.Modules.Posts.Tests.Integration;

/// <summary>Test host for the Posts module: applies its migrations and swaps the media integration for a fake.</summary>
public sealed class PostsApp : BoilerplateApp
{
    public FakeMediaClient Media { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IMediaClient>();
            services.AddSingleton<IMediaClient>(Media);
        });
    }

    protected override async Task RunMigrationsAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PostWriteDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
