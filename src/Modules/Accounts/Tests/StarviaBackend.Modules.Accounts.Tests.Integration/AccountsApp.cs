using System.Runtime.CompilerServices;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Initializers;
using StarviaBackend.Shared.Tests.Integration;

namespace StarviaBackend.Modules.Accounts.Tests.Integration;

/// <summary>Test host for the Accounts module: applies its migrations, seeds roles, starts the bus.</summary>
public sealed class AccountsApp : BoilerplateApp
{
    protected override async Task RunMigrationsAsync()
    {
        using (var scope = Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AccountsWriteDbContext>();
            await dbContext.Database.MigrateAsync();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            await AccountsSeeder.SeedRolesAsync(roleManager);
        }

        // Hosted services (incl. the MassTransit bus) were stripped by the base host; start the bus
        // manually so publishing from command handlers works during tests.
        await Services.GetRequiredService<IBusControl>().StartAsync();
    }
}
