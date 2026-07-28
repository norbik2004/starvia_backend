using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Platforms.Configurations.Read;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Platforms.Configurations.Read.Models;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;

/// <summary>
/// Read side: no-tracking context exposing lightweight projections. Maps onto the same physical
/// tables as the write context but only the columns queries need.
/// </summary>
internal sealed class PlatformsReadDbContext(DbContextOptions<PlatformsReadDbContext> options) : DbContext(options)
{
    public DbSet<UserReadModel> Users => Set<UserReadModel>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(PlatformsWriteDbContext.Schema);
        builder.ApplyConfiguration(new UserReadConfiguration());
    }

    public override int SaveChanges() => throw new InvalidOperationException("Read context is read-only.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Read context is read-only.");
}
