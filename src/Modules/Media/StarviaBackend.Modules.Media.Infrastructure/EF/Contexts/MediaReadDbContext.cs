using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Media.Infrastructure.EF.MediaFiles.Configurations.Read;
using StarviaBackend.Modules.Media.Infrastructure.EF.MediaFiles.Configurations.Read.Models;

namespace StarviaBackend.Modules.Media.Infrastructure.EF.Contexts;

internal sealed class MediaReadDbContext(DbContextOptions<MediaReadDbContext> options) : DbContext(options)
{
    public DbSet<MediaFileReadModel> MediaFiles => Set<MediaFileReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(MediaWriteDbContext.Schema);
        modelBuilder.ApplyConfiguration(new MediaFileReadConfiguration());
    }

    public override int SaveChanges() => throw new InvalidOperationException("Read context is read-only.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Read context is read-only.");
}
