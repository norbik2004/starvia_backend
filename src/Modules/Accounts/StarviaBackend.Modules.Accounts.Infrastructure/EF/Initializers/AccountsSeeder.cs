using Microsoft.AspNetCore.Identity;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Time;

namespace StarviaBackend.Modules.Accounts.Infrastructure.EF.Initializers;

internal static class AccountsSeeder
{
    /// <summary>Ensures the well-known roles exist. Idempotent — safe to call repeatedly.</summary>
    public static async Task SeedRolesAsync(RoleManager<Role> roleManager)
    {
        foreach (var roleName in new[] { UserRoles.Admin, UserRoles.User })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new Role(roleName));
            }
        }
    }


    public static async Task SeedAdminAsync(UserManager<User> userManager, IClock clock)
    {
        var adminEmail = "admin@admin.com";
#pragma warning disable S2068
        var adminPassword = "Admin123!";
#pragma warning restore S2068

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin == null)
        {
            var adminUser = User.Create(adminEmail, clock.UtcNow);

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
            }
            else
            {
                throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}
