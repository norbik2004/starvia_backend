using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Shared.Abstractions.Posts;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Configurations.Write;

internal sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Title).IsRequired().HasMaxLength(75);
        builder.Property(p => p.Body).HasMaxLength(PostLimits.MaxBodyLength);
        builder.Property(p => p.Status).IsRequired();

        builder.HasMany(p => p.PostPublications)
               .WithOne(pp => pp.Post)
               .HasForeignKey(pp => pp.PostId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
