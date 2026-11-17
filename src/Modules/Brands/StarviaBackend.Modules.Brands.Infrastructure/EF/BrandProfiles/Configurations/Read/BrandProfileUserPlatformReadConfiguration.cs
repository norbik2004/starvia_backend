using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Configurations.Read.Models;

namespace StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Configurations.Read;

internal sealed class BrandProfileUserPlatformReadConfiguration
    : IEntityTypeConfiguration<BrandProfileUserPlatformReadModel>
{
    public void Configure(EntityTypeBuilder<BrandProfileUserPlatformReadModel> builder)
    {
        builder.ToTable("BrandProfileUserPlatforms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BrandProfileId);
        builder.Property(x => x.UserPlatformId);
        builder.Property(x => x.IsDefault);
    }
}
