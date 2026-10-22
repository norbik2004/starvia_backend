using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Emails.Infrastructure.EF.Emails.Configurations.Read;
using StarviaBackend.Modules.Emails.Infrastructure.EF.Emails.Configurations.Read.Models;

namespace StarviaBackend.Modules.Emails.Infrastructure.EF.Contexts;

internal sealed class EmailsReadDbContex(DbContextOptions<EmailsReadDbContex> options) : DbContext(options)
{
    public DbSet<EmailReadModel> Emails => Set<EmailReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(EmailsWriteDbContext.Schema);
        modelBuilder.ApplyConfiguration(new EmailReadConfiguration());
    }

    public override int SaveChanges() => throw new InvalidOperationException("Read context is read-only.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Read context is read-only.");
}
