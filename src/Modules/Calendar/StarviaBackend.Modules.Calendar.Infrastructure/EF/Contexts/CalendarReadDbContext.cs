using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.CalendarEntries.Configurations.Read;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.CalendarEntries.Configurations.Read.Models;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.ContentCalendars.Configurations.Read;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.ContentCalendars.Configurations.Read.Models;

namespace StarviaBackend.Modules.Calendar.Infrastructure.EF.Contexts;

internal sealed class CalendarReadDbContext(DbContextOptions<CalendarReadDbContext> options) : DbContext(options)
{
    public DbSet<ContentCalendarReadModel> ContentCalendars => Set<ContentCalendarReadModel>();
    public DbSet<CalendarEntryReadModel> CalendarEntries => Set<CalendarEntryReadModel>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(CalendarWriteDbContext.Schema);
        modelBuilder.ApplyConfiguration(new CalendarEntryReadConfiguration());
        modelBuilder.ApplyConfiguration(new ContentCalendarReadConfiguration());
    }

    public override int SaveChanges() => throw new InvalidOperationException("Read context is read-only.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Read context is read-only.");
}
