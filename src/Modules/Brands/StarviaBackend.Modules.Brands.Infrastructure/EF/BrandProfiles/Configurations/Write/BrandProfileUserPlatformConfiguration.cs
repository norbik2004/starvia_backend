using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Entities;

namespace StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Configurations.Write;

internal sealed class BrandProfileUserPlatformConfiguration : IEntityTypeConfiguration<BrandProfileUserPlatform>
{
    public void Configure(EntityTypeBuilder<BrandProfileUserPlatform> builder)
    {
        builder.ToTable("BrandProfileUserPlatforms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.BrandProfileId).IsRequired();
        builder.Property(x => x.UserPlatformId).IsRequired();
        builder.Property(x => x.IsDefault).IsRequired();

        builder.HasIndex(x => new { x.BrandProfileId, x.UserPlatformId }).IsUnique();
        builder.HasIndex(x => x.UserPlatformId);
    }
}
