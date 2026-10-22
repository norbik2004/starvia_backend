using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Posts.Infrastructure.EF.PostPublications.Configurations.Read.Models;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.PostPublications.Configurations.Read;

internal sealed class PostPublicationReadConfiguration : IEntityTypeConfiguration<PostPublicationReadModel>
{
    public void Configure(EntityTypeBuilder<PostPublicationReadModel> builder)
    {
        builder.ToTable("PostPublications");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.PostId).IsRequired();
        builder.Property(u => u.Status).IsRequired();
        builder.Property(u => u.PublishedAt);
        builder.Property(u => u.ExternalPostId);
        builder.Property(u => u.UserPlatformId).IsRequired();
    }
}
