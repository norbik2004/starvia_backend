using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Platforms.Configurations.Read;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Platforms.Configurations.Read.Models;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.UserPlatforms.Configurations.Read;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.UserPlatforms.Configurations.Read.Models;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;

/// <summary>
/// Read side: no-tracking context exposing lightweight projections. Maps onto the same physical
/// tables as the write context but only the columns queries need.
/// </summary>
internal sealed class PlatformsReadDbContext(DbContextOptions<PlatformsReadDbContext> options) : DbContext(options)
{
    public DbSet<PlatformReadModel> Platforms => Set<PlatformReadModel>();
    public DbSet<UserPlatformReadModel> UserPlatforms => Set<UserPlatformReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(PlatformsWriteDbContext.Schema);
        modelBuilder.ApplyConfiguration(new PlatformReadConfiguration());
        modelBuilder.ApplyConfiguration(new UserPlatformReadConfiguration());
    }

    public override int SaveChanges() => throw new InvalidOperationException("Read context is read-only.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Read context is read-only.");
}
