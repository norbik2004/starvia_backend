using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Configurations.Read.Models;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Configurations.Read;

internal sealed class PostReadConfiguration : IEntityTypeConfiguration<PostReadModel>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<PostReadModel> builder)
    {
        builder.ToTable("Posts");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Title);
        builder.Property(p => p.Body);
        builder.Property(p => p.Status);
        builder.Property(p => p.Platforms)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<PlatformType>>(v, JsonOptions) ?? new List<PlatformType>());
        builder.Property(p => p.CreatedAt);
        builder.Property(p => p.CreatedBy);
        builder.Property(p => p.LastModifiedAt);

        builder.HasMany(p => p.PostAttachments)
               .WithOne()
               .HasForeignKey(pp => pp.PostId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.PostPublications)
               .WithOne()
               .HasForeignKey(pp => pp.PostId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
