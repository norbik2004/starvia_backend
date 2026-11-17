using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Configurations.Read.Models;

namespace StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Configurations.Read;

internal sealed class BrandProfilePlatformReadConfiguration : IEntityTypeConfiguration<BrandProfilePlatformReadModel>
{
    public void Configure(EntityTypeBuilder<BrandProfilePlatformReadModel> builder)
    {
        builder.ToTable("BrandProfilePlatforms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BrandProfileId);
        builder.Property(x => x.PlatformType);
    }
}
