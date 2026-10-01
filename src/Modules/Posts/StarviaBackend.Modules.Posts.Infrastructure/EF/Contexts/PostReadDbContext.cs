using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Posts.Infrastructure.EF.PostPublications.Configurations.Read;
using StarviaBackend.Modules.Posts.Infrastructure.EF.PostPublications.Configurations.Read.Models;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Configurations.Read;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Configurations.Read.Models;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.Contexts;

internal sealed class PostReadDbContext(DbContextOptions<PostReadDbContext> options) : DbContext(options)
{
    public DbSet<PostReadModel> Posts => Set<PostReadModel>();
    public DbSet<PostPublicationReadModel> PostPublications => Set<PostPublicationReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(PostWriteDbContext.Schema);
        modelBuilder.ApplyConfiguration(new PostReadConfiguration());
        modelBuilder.ApplyConfiguration(new PostPublicationReadConfiguration());
    }

    public override int SaveChanges() => throw new InvalidOperationException("Read context is read-only.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Read context is read-only.");
}
