using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Configurations.Read;
using StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Configurations.Read.Models;

namespace StarviaBackend.Modules.Brands.Infrastructure.EF.Contexts;

internal sealed class BrandsReadDbContext(DbContextOptions<BrandsReadDbContext> options) : DbContext(options)
{
    public DbSet<BrandProfileReadModel> BrandProfiles => Set<BrandProfileReadModel>();
    public DbSet<BrandProfileUserPlatformReadModel> BrandProfileUserPlatforms => Set<BrandProfileUserPlatformReadModel>();
    public DbSet<BrandProfilePlatformReadModel> BrandProfilePlatforms => Set<BrandProfilePlatformReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(BrandsWriteDbContext.Schema);
        modelBuilder.ApplyConfiguration(new BrandProfileReadConfiguration());
        modelBuilder.ApplyConfiguration(new BrandProfileUserPlatformReadConfiguration());
        modelBuilder.ApplyConfiguration(new BrandProfilePlatformReadConfiguration());
    }

    public override int SaveChanges() => throw new InvalidOperationException("Read context is read-only.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Read context is read-only.");
}
