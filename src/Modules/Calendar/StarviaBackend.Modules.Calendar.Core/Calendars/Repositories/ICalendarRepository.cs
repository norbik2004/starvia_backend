using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Calendar.Core.Calendars.Entities;

namespace StarviaBackend.Modules.Calendar.Core.Calendars.Repositories;

internal interface ICalendarRepository
{
    Task AddAsync(ContentCalendar calendar, CancellationToken cancellationToken = default);
    Task<ContentCalendar?> GetByIdAsync(Guid calendarId, CancellationToken cancellationToken = default);
    Task RemoveAsync(ContentCalendar calendar, CancellationToken cancellationToken = default);
    Task UpdateAsync(ContentCalendar calendar, CancellationToken cancellationToken = default);
    Task<ContentCalendar?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
