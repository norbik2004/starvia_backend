using Microsoft.AspNetCore.Identity;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Shared.Abstractions.Auth;

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
}
