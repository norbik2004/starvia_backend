using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Media.Core.Media.Entities;
using StarviaBackend.Modules.Media.Infrastructure.EF.MediaFiles.Configurations.Write;

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
