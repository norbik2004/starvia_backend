using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Calendar.Core.Calendars.Entities;
using StarviaBackend.Modules.Calendar.Core.Calendars.Repositories;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Calendar.Infrastructure.EF.ContentCalendars.Repositories;

internal class ContentCalendarRepository(CalendarWriteDbContext dbContext) : ICalendarRepository
{
    private readonly DbSet<ContentCalendar> _contentCalendars = dbContext.ContentCalendars;

    public async Task AddAsync(ContentCalendar calendar, CancellationToken cancellationToken = default)
    {
        await _contentCalendars.AddAsync(calendar, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ContentCalendar?> GetByIdAsync(Guid calendarId, CancellationToken cancellationToken = default)
    {
        return await _contentCalendars
            .Include(c => c.CalendarEntries)
            .FirstOrDefaultAsync(c => c.Id == calendarId, cancellationToken);
    }

    public async Task<ContentCalendar?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _contentCalendars
            .Include(c => c.CalendarEntries)
            .FirstOrDefaultAsync(c => c.CreatedBy == userId.ToString(), cancellationToken);
    }

    public async Task RemoveAsync(ContentCalendar calendar, CancellationToken cancellationToken = default)
    {
        _contentCalendars.Remove(calendar);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ContentCalendar calendar, CancellationToken cancellationToken = default)
    {
        _contentCalendars.Update(calendar);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
