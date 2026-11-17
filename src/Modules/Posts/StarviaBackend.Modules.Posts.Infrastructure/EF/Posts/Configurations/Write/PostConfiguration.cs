using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Shared.Abstractions.Platforms;
using StarviaBackend.Shared.Abstractions.Posts;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Configurations.Write;

internal sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Title).IsRequired().HasMaxLength(75);
        builder.Property(p => p.Body).HasMaxLength(PostLimits.MaxBodyLength);
        builder.Property(p => p.Status).IsRequired();

        builder.Property(p => p.Platforms)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<PlatformType>>(v, JsonOptions) ?? new List<PlatformType>())
            .Metadata.SetValueComparer(CreatePlatformsComparer());

        builder.HasMany(p => p.PostPublications)
               .WithOne(pp => pp.Post)
               .HasForeignKey(pp => pp.PostId)
               .OnDelete(DeleteBehavior.Cascade);
    }

    private static ValueComparer<IReadOnlyList<PlatformType>> CreatePlatformsComparer() =>
        new(
            (a, b) => JsonSerializer.Serialize(a, JsonOptions) == JsonSerializer.Serialize(b, JsonOptions),
            v => JsonSerializer.Serialize(v, JsonOptions).GetHashCode(),
            v => JsonSerializer.Deserialize<List<PlatformType>>(JsonSerializer.Serialize(v, JsonOptions), JsonOptions)!);
}
