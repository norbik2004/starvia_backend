using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Calendar.Core.Calendars.Entities;
using StarviaBackend.Modules.Calendar.Core.Calendars.Repositories;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Calendar.Infrastructure.EF.CalendarEntries.Repositories;

internal class CalendarEntryRepository(CalendarWriteDbContext dbContext) : ICalendarEntryRepository
{
    private readonly DbSet<CalendarEntry> _calendarEntries = dbContext.CalendarEntries;

    public async Task AddAsync(CalendarEntry calendar, CancellationToken cancellationToken = default)
    {
        await _calendarEntries.AddAsync(calendar, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<CalendarEntry?> GetByIdAsync(Guid calendarEntryId, CancellationToken cancellationToken = default)
    {
        return _calendarEntries.FirstOrDefaultAsync(c => c.Id == calendarEntryId, cancellationToken);
    }

    public async Task RemoveAsync(CalendarEntry calendar, CancellationToken cancellationToken = default)
    {
        _calendarEntries.Remove(calendar);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(CalendarEntry calendar, CancellationToken cancellationToken = default)
    {
        _calendarEntries.Update(calendar);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
