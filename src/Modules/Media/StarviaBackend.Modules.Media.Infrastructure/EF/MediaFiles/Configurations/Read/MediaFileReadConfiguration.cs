using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Media.Infrastructure.EF.MediaFiles.Configurations.Read.Models;

namespace StarviaBackend.Modules.Media.Infrastructure.EF.MediaFiles.Configurations.Read;

internal class MediaFileReadConfiguration : IEntityTypeConfiguration<MediaFileReadModel>
{
    public void Configure(EntityTypeBuilder<MediaFileReadModel> builder)
    {
        builder.ToTable("MediaFiles");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.FilePath);
        builder.Property(m => m.FileName);
        builder.Property(m => m.Type);
        builder.Property(m => m.Source);
        builder.Property(m => m.Description);
    }
}
