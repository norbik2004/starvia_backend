using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.PostPublications.Configurations.Write;

internal sealed class PostPublicationConfiguration : IEntityTypeConfiguration<PostPublication>
{
    public void Configure(EntityTypeBuilder<PostPublication> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.PostId).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.PublishedAt);
        builder.Property(x => x.ExternalPostId);
        builder.Property(x => x.UserPlatformId).IsRequired();

        builder.HasOne(x => x.Post)
            .WithMany()
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
