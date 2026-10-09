using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.MediaFiles.Core.Media.Entities;

namespace StarviaBackend.Modules.MediaFiles.Infrastructure.EF.MediaFiles.Configurations.Write;

internal sealed class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.ToTable("MediaFiles");
        builder.HasKey(mf => mf.Id);
        builder.Property(mf => mf.Id).ValueGeneratedNever();
        builder.Property(mf => mf.FileName).IsRequired().HasMaxLength(100);
        builder.Property(mf => mf.FilePath).IsRequired().HasMaxLength(255);
        builder.Property(mf => mf.Description).HasMaxLength(500);
        builder.Property(mf => mf.Source).IsRequired();
        builder.Property(mf => mf.Type).IsRequired();
    }
}
