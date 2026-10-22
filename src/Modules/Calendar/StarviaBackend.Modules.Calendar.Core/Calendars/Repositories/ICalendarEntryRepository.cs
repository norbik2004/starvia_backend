using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Calendar.Core.Calendars.Entities;

namespace StarviaBackend.Modules.Calendar.Core.Calendars.Repositories;

internal interface ICalendarEntryRepository
{
    Task AddAsync(CalendarEntry calendar, CancellationToken cancellationToken = default);
    Task<CalendarEntry?> GetByIdAsync(Guid calendarEntryId, CancellationToken cancellationToken = default);
    Task RemoveAsync(CalendarEntry calendar, CancellationToken cancellationToken = default);
    Task UpdateAsync(CalendarEntry calendar, CancellationToken cancellationToken = default);
}
