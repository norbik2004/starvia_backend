using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Platforms.Configurations.Write;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.UserPlatforms.Configurations.Write;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;

/// <summary>
/// Write side: full ASP.NET Identity model (custom entities incl. claims/logins/tokens) mapped
/// into the module's own <c>accounts</c> schema. Per-entity mapping lives in Configurations/Write.
/// </summary>
internal sealed class PlatformsWriteDbContext(DbContextOptions<PlatformsWriteDbContext> options) : DbContext(options)
{
    public const string Schema = "platforms";

    public DbSet<Platform> Platforms => Set<Platform>();

    public DbSet<UserPlatform> UserPlatforms => Set<UserPlatform>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.ApplyConfiguration(new PlatformConfiguration());
        modelBuilder.ApplyConfiguration(new UserPlatformConfiguration());
    }
}
