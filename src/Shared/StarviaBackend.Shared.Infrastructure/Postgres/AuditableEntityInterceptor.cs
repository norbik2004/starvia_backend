using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Domain;
using StarviaBackend.Shared.Abstractions.Time;

namespace StarviaBackend.Shared.Infrastructure.Postgres;

/// <summary>Fills audit columns on <see cref="IAuditable"/> entities on save.</summary>
public sealed class AuditableEntityInterceptor(IClock clock, IContext context) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void Apply(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var now = clock.UtcNow;
        var user = context.Identity.UserId?.ToString();

        foreach (var entry in dbContext.ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = user;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedAt = now;
                    entry.Entity.LastModifiedBy = user;
                    break;
            }
        }
    }
}
