using System;
using System.Collections.Generic;
using System.Text;
using MassTransit.Configuration;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.MediaFiles.Core.Media.Entities;
using StarviaBackend.Modules.MediaFiles.Infrastructure.EF.MediaFiles.Configurations.Write;

namespace StarviaBackend.Modules.Media.Infrastructure.EF.Contexts;

internal sealed class MediaWriteDbContext(DbContextOptions<MediaWriteDbContext> options) : DbContext(options)
{
    public const string Schema = "media";
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.ApplyConfiguration(new MediaFileConfiguration());
    }
}
