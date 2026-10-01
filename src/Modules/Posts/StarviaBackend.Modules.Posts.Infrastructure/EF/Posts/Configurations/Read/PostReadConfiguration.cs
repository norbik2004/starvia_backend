using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Configurations.Read.Models;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Configurations.Read;

internal sealed class PostReadConfiguration : IEntityTypeConfiguration<PostReadModel>
{
    public void Configure(EntityTypeBuilder<PostReadModel> builder)
    {
        builder.ToTable("Posts");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Title);
        builder.Property(p => p.Body);
        builder.Property(p => p.Status);
        builder.Property(p => p.CreatedAt);
    }
}
