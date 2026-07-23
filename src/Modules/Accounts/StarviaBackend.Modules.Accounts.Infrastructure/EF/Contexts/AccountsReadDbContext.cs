using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Users.Configurations.Read;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Users.Configurations.Read.Models;

namespace StarviaBackend.Modules.Accounts.Infrastructure.EF.Contexts;

/// <summary>
/// Read side: no-tracking context exposing lightweight projections. Maps onto the same physical
/// tables as the write context but only the columns queries need.
/// </summary>
internal sealed class AccountsReadDbContext(DbContextOptions<AccountsReadDbContext> options) : DbContext(options)
{
    public DbSet<UserReadModel> Users => Set<UserReadModel>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(AccountsWriteDbContext.Schema);
        builder.ApplyConfiguration(new UserReadConfiguration());
    }

    public override int SaveChanges() => throw new InvalidOperationException("Read context is read-only.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Read context is read-only.");
}
