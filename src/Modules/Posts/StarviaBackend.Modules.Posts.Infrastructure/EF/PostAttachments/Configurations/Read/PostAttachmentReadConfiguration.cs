using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Posts.Infrastructure.EF.PostAttachments.Configurations.Read.Models;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.PostAttachments.Configurations.Read;

internal sealed class PostAttachmentReadConfiguration : IEntityTypeConfiguration<PostAttachmentReadModel>
{
    public void Configure(EntityTypeBuilder<PostAttachmentReadModel> builder)
    {
        builder.ToTable("PostAttachments");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).IsRequired();
        builder.Property(u => u.PostId).IsRequired();
        builder.Property(u => u.FileId).IsRequired();
        builder.Property(u => u.Order).IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();
    }
}
