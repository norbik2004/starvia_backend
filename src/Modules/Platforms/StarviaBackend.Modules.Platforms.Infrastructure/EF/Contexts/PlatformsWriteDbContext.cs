using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Platforms.Core.Users.Entities;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Users.Configurations.Write;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;

/// <summary>
/// Write side: full ASP.NET Identity model (custom entities incl. claims/logins/tokens) mapped
/// into the module's own <c>accounts</c> schema. Per-entity mapping lives in Configurations/Write.
/// </summary>
internal sealed class PlatformsWriteDbContext(DbContextOptions<PlatformsWriteDbContext> options) : DbContext(options)
{
    public const string Schema = "posts";

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
