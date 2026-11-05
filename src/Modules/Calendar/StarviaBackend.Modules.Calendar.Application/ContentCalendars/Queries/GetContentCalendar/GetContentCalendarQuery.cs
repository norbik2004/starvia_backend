using StarviaBackend.Modules.Calendar.Core.Calendars.Enums;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Calendar.Application.ContentCalendars.Queries.GetContentCalendar;

/// <summary>The calendar created by <paramref name="UserId"/> (the caller's own, see the endpoint). Entries come ordered by schedule.</summary>
internal sealed record GetMyContentCalendarQuery(Guid UserId) : IQuery<ContentCalendarLongDto>;

/// <summary>One calendar by id, if the caller may view it (see the calendar access policy).</summary>
internal sealed record GetContentCalendarQuery(Guid CalendarId) : IQuery<ContentCalendarLongDto>;

public sealed record ContentCalendarLongDto(Guid Id, string Name, DateTime CreatedAt, ICollection<CalendarEntryLongDto> CalendarEntries);
public sealed record CalendarEntryLongDto(Guid id, Guid PostId, DateTimeOffset ScheduledAt, CalendarEntryStatus Status);
