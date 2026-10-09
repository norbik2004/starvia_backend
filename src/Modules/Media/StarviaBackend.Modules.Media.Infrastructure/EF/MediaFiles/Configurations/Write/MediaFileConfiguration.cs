using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarviaBackend.Modules.Media.Core.Media.Entities;

namespace StarviaBackend.Modules.Media.Infrastructure.EF.MediaFiles.Configurations.Write;

internal sealed class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.ToTable("MediaFiles");
        builder.HasKey(mf => mf.Id);
        builder.Property(mf => mf.Id).ValueGeneratedNever();
        builder.Property(mf => mf.FileName).HasMaxLength(100);
        builder.Property(mf => mf.FilePath).HasMaxLength(255);
        builder.Property(mf => mf.Description).HasMaxLength(500);
        builder.Property(mf => mf.Source);
        builder.Property(mf => mf.Type);
    }
}
