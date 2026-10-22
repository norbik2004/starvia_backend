using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Time;

namespace StarviaBackend.Modules.Accounts.Infrastructure.EF.Initializers;

/// <summary>
/// On startup: applies pending migrations and seeds roles. Removed in integration tests
/// (all hosted services are stripped there); tests run the same steps deterministically.
/// </summary>
internal sealed class AccountsDataInitializer(IServiceProvider serviceProvider, IClock clock, ILogger<AccountsDataInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AccountsWriteDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        await AccountsSeeder.SeedRolesAsync(roleManager);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var enviroment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        if(enviroment != null && enviroment.IsDevelopment())
        {
            logger.LogInformation("Seeding admin user for development environment.");

            await AccountsSeeder.SeedAdminAsync(userManager, clock);
            await AccountsSeeder.SeedUserAsync(userManager, clock);
        }
    }
    

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
