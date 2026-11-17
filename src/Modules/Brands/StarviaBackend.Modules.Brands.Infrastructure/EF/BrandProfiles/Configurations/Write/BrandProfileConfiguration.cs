using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Entities;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Models;

namespace StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Configurations.Write;

internal sealed class BrandProfileConfiguration : IEntityTypeConfiguration<BrandProfile>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<BrandProfile> builder)
    {
        builder.ToTable("BrandProfiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(BrandProfile.MaxNameLength).IsRequired();

        builder.Property(x => x.Brief)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<BrandBrief>(v, JsonOptions)!)
            .Metadata.SetValueComparer(CreateBriefComparer());

        builder.Property(x => x.GenerationDefaults)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, JsonOptions),
                v => v == null ? null : JsonSerializer.Deserialize<BrandGenerationDefaults>(v, JsonOptions))
            .Metadata.SetValueComparer(CreateGenerationDefaultsComparer());

        builder.HasMany(x => x.UserPlatforms)
            .WithOne(x => x.BrandProfile)
            .HasForeignKey(x => x.BrandProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Platforms)
            .WithOne(x => x.BrandProfile)
            .HasForeignKey(x => x.BrandProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.UserPlatforms).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Platforms).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.UserId);
    }

    private static ValueComparer<BrandBrief> CreateBriefComparer() =>
        new(
            (a, b) => JsonSerializer.Serialize(a, JsonOptions) == JsonSerializer.Serialize(b, JsonOptions),
            v => JsonSerializer.Serialize(v, JsonOptions).GetHashCode(),
            v => JsonSerializer.Deserialize<BrandBrief>(JsonSerializer.Serialize(v, JsonOptions), JsonOptions)!);

    private static ValueComparer<BrandGenerationDefaults?> CreateGenerationDefaultsComparer() =>
        new(
            (a, b) => JsonSerializer.Serialize(a, JsonOptions) == JsonSerializer.Serialize(b, JsonOptions),
            v => v == null ? 0 : JsonSerializer.Serialize(v, JsonOptions).GetHashCode(),
            v => v == null
                ? null
                : JsonSerializer.Deserialize<BrandGenerationDefaults>(JsonSerializer.Serialize(v, JsonOptions), JsonOptions));
}
