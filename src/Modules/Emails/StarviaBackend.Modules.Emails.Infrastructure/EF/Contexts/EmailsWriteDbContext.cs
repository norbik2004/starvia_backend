using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Emails.Core.Emails.Entities;
using StarviaBackend.Modules.Emails.Infrastructure.EF.Emails.Configurations.Write;

namespace StarviaBackend.Modules.Emails.Infrastructure.EF.Contexts;

internal sealed class EmailsWriteDbContext(DbContextOptions<EmailsWriteDbContext> options) : DbContext(options)
{
    public const string Schema = "emails";

    public DbSet<Email> Emails => Set<Email>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.ApplyConfiguration(new EmailConfiguration());
    }
}
