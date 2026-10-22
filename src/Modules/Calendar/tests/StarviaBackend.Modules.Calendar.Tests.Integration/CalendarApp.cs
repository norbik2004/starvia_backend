using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Posts.Integrations;
using StarviaBackend.Shared.Tests.Integration;

namespace StarviaBackend.Modules.Calendar.Tests.Integration;

/// <summary>Test host for the Calendar module: applies its migrations and swaps the posts integration for a fake.</summary>
public sealed class CalendarApp : BoilerplateApp
{
    public FakePostsClient Posts { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IPostsClient>();
            services.AddSingleton<IPostsClient>(Posts);
        });
    }

    protected override async Task RunMigrationsAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CalendarWriteDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
