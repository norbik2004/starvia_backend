using StarviaBackend.Modules.Calendar.Core.Calendars.Entities;
using StarviaBackend.Modules.Calendar.Core.Calendars.Repositories;

namespace StarviaBackend.Modules.Calendar.Tests.Unit.Fakes;

internal sealed class InMemoryCalendarRepository : ICalendarRepository
{
    public List<ContentCalendar> Calendars { get; } = [];
    public List<ContentCalendar> Updated { get; } = [];
    public List<ContentCalendar> Removed { get; } = [];

    public Task AddAsync(ContentCalendar calendar, CancellationToken cancellationToken = default)
    {
        Calendars.Add(calendar);
        return Task.CompletedTask;
    }

    public Task<ContentCalendar?> GetByIdAsync(Guid calendarId, CancellationToken cancellationToken = default)
        => Task.FromResult(Calendars.FirstOrDefault(c => c.Id == calendarId));

    public Task RemoveAsync(ContentCalendar calendar, CancellationToken cancellationToken = default)
    {
        Calendars.Remove(calendar);
        Removed.Add(calendar);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ContentCalendar calendar, CancellationToken cancellationToken = default)
    {
        Updated.Add(calendar);
        return Task.CompletedTask;
    }

    public Task<ContentCalendar?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => Task.FromResult(Calendars.FirstOrDefault(c => c.CreatedBy == userId.ToString()));

    /// <summary>Seeds a calendar "created" by the given user, as the audit interceptor would.</summary>
    public ContentCalendar Seed(Guid ownerId, string name = "Seeded calendar")
    {
        var calendar = ContentCalendar.Create(name);
        calendar.CreatedBy = ownerId.ToString();
        Calendars.Add(calendar);
        return calendar;
    }
}
