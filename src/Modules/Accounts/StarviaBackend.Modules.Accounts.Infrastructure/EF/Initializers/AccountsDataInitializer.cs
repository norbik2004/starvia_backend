using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Accounts.Infrastructure.EF.Initializers;

/// <summary>
/// On startup: applies pending migrations and seeds roles. Removed in integration tests
/// (all hosted services are stripped there); tests run the same steps deterministically.
/// </summary>
internal sealed class AccountsDataInitializer(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AccountsWriteDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        await AccountsSeeder.SeedRolesAsync(roleManager);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
