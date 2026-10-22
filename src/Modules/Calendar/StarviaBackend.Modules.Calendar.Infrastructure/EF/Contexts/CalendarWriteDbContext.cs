using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Calendar.Core.Calendars.Entities;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.CalendarEntries.Configurations.Write;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.ContentCalendars.Configurations.Write;

namespace StarviaBackend.Modules.Calendar.Infrastructure.EF.Contexts;

internal sealed class CalendarWriteDbContext(DbContextOptions<CalendarWriteDbContext> options) : DbContext(options)
{
    public const string Schema = "calendars";
    public DbSet<CalendarEntry> CalendarEntries => Set<CalendarEntry>();
    public DbSet<ContentCalendar> ContentCalendars => Set<ContentCalendar>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.ApplyConfiguration(new CalendarEntryConfiguration());
        modelBuilder.ApplyConfiguration(new ContentCalendarConfiguration());
    }
}
