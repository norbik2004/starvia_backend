using StarviaBackend.Modules.Calendar.Core.Calendars.Entities;
using StarviaBackend.Modules.Calendar.Core.Calendars.Repositories;

namespace StarviaBackend.Modules.Calendar.Tests.Unit.Fakes;

internal sealed class InMemoryCalendarEntryRepository : ICalendarEntryRepository
{
    public List<CalendarEntry> Entries { get; } = [];

    public Task AddAsync(CalendarEntry calendar, CancellationToken cancellationToken = default)
    {
        Entries.Add(calendar);
        return Task.CompletedTask;
    }

    public Task<CalendarEntry?> GetByIdAsync(Guid calendarEntryId, CancellationToken cancellationToken = default)
        => Task.FromResult(Entries.FirstOrDefault(e => e.Id == calendarEntryId));

    public Task RemoveAsync(CalendarEntry calendar, CancellationToken cancellationToken = default)
    {
        Entries.Remove(calendar);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(CalendarEntry calendar, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
