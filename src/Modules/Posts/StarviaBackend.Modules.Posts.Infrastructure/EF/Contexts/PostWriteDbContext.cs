using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Infrastructure.EF.PostPublications.Configurations.Write;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Configurations.Write;

namespace StarviaBackend.Modules.Posts.Infrastructure.EF.Contexts;

internal sealed class PostWriteDbContext(DbContextOptions<PostWriteDbContext> options) : DbContext(options)
{
    public const string Schema = "posts";
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostPublication> PostPublications => Set<PostPublication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.ApplyConfiguration(new PostConfiguration());
        modelBuilder.ApplyConfiguration(new PostPublicationConfiguration());
    }
}
