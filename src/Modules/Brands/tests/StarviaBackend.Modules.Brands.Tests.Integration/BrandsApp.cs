using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Brands.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Tests.Integration;

namespace StarviaBackend.Modules.Brands.Tests.Integration;

/// <summary>Test host for the Brands module: applies its migrations.</summary>
public sealed class BrandsApp : BoilerplateApp
{
    protected override async Task RunMigrationsAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BrandsWriteDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
