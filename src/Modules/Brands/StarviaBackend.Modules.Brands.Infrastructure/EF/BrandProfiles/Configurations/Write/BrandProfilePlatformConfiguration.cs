using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Entities;

namespace StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Configurations.Write;

internal sealed class BrandProfilePlatformConfiguration : IEntityTypeConfiguration<BrandProfilePlatform>
{
    public void Configure(EntityTypeBuilder<BrandProfilePlatform> builder)
    {
        builder.ToTable("BrandProfilePlatforms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.BrandProfileId).IsRequired();
        builder.Property(x => x.PlatformType).IsRequired();

        builder.HasIndex(x => new { x.BrandProfileId, x.PlatformType }).IsUnique();
        builder.HasIndex(x => x.PlatformType);
    }
}
