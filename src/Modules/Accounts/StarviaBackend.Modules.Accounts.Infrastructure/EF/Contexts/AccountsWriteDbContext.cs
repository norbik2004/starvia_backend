using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Users.Configurations.Write;

namespace StarviaBackend.Modules.Accounts.Infrastructure.EF.Contexts;

/// <summary>
/// Write side: full ASP.NET Identity model (custom entities incl. claims/logins/tokens) mapped
/// into the module's own <c>accounts</c> schema. Per-entity mapping lives in Configurations/Write.
/// </summary>
internal sealed class AccountsWriteDbContext(DbContextOptions<AccountsWriteDbContext> options)
    : IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>(options)
{
    public const string Schema = "accounts";

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema(Schema);

        builder.ApplyConfiguration(new UserConfiguration());
        builder.ApplyConfiguration(new RoleConfiguration());
        builder.ApplyConfiguration(new UserRoleConfiguration());
        builder.ApplyConfiguration(new UserClaimConfiguration());
        builder.ApplyConfiguration(new RoleClaimConfiguration());
        builder.ApplyConfiguration(new UserLoginConfiguration());
        builder.ApplyConfiguration(new UserTokenConfiguration());
    }
}
