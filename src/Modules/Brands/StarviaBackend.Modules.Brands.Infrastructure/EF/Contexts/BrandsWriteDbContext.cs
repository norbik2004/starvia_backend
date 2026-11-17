using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Entities;
using StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Configurations.Write;

namespace StarviaBackend.Modules.Brands.Infrastructure.EF.Contexts;

internal sealed class BrandsWriteDbContext(DbContextOptions<BrandsWriteDbContext> options) : DbContext(options)
{
    public const string Schema = "brands";

    public DbSet<BrandProfile> BrandProfiles => Set<BrandProfile>();
    public DbSet<BrandProfileUserPlatform> BrandProfileUserPlatforms => Set<BrandProfileUserPlatform>();
    public DbSet<BrandProfilePlatform> BrandProfilePlatforms => Set<BrandProfilePlatform>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.ApplyConfiguration(new BrandProfileConfiguration());
        modelBuilder.ApplyConfiguration(new BrandProfileUserPlatformConfiguration());
        modelBuilder.ApplyConfiguration(new BrandProfilePlatformConfiguration());
    }
}
