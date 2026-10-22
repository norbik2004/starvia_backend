using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StarviaBackend.Modules.Media.Core.Media.Storage;
using StarviaBackend.Modules.Media.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Tests.Integration;

namespace StarviaBackend.Modules.Media.Tests.Integration;

/// <summary>Test host for the Media module: applies its migrations and swaps the object storage for an in-memory fake.</summary>
public sealed class MediaApp : BoilerplateApp
{
    internal InMemoryFileStorageService Storage { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IFileStorageService>();
            services.AddSingleton<IFileStorageService>(Storage);
        });
    }

    protected override async Task RunMigrationsAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MediaWriteDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
