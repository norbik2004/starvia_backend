using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Models;
using StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Configurations.Read.Models;

namespace StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Configurations.Read;

internal sealed class BrandProfileReadConfiguration : IEntityTypeConfiguration<BrandProfileReadModel>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<BrandProfileReadModel> builder)
    {
        builder.ToTable("BrandProfiles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Brief)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<BrandBrief>(v, JsonOptions)!);

        builder.Property(x => x.GenerationDefaults)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, JsonOptions),
                v => v == null ? null : JsonSerializer.Deserialize<BrandGenerationDefaults>(v, JsonOptions));

        builder.HasMany(x => x.UserPlatforms)
            .WithOne(x => x.BrandProfile)
            .HasForeignKey(x => x.BrandProfileId);

        builder.HasMany(x => x.Platforms)
            .WithOne(x => x.BrandProfile)
            .HasForeignKey(x => x.BrandProfileId);
    }
}
