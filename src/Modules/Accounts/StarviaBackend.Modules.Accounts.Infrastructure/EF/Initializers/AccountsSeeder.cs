using Microsoft.AspNetCore.Identity;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Core.Users.Exceptions;
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
            adminUser.EmailConfirmed = true;

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
            }
            else
            {
                throw new UserCreationFailedException($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }

    public static async Task SeedUserAsync(UserManager<User> userManager, IClock clock)
    {
        var userEmail = "Uzytkownik@test1.com";

        #pragma warning disable S2068
        var userPassword = "Uzytkownik1";
        #pragma warning restore S2068

        var existingUser = await userManager.FindByEmailAsync(userEmail);

        if(existingUser == null)
        {
            var user = User.Create(userEmail, clock.UtcNow);
            user.EmailConfirmed = true;
            var result = await userManager.CreateAsync(user, userPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, UserRoles.User);
            }
            else
            {
                throw new UserCreationFailedException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}
