using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Queries.GetContentCalendar;
using StarviaBackend.Modules.Calendar.Core.Calendars.Enums;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Calendar.Application.CalednarEntry.Queries.BrowseCalendarEntries;

/// <summary>
/// Pages through the entries of one calendar, soonest first by default. The caller needs to be
/// allowed to view the calendar's entries (see the calendar access policy).
/// </summary>
/// <param name="Status">Only entries in this status.</param>
/// <param name="From">Only entries scheduled at or after this moment.</param>
/// <param name="To">Only entries scheduled before this moment.</param>
/// <param name="Descending">Latest first when true.</param>
internal sealed record BrowseCalendarEntriesQuery(
    Guid CalendarId,
    int Page = 1,
    int PageSize = 20,
    CalendarEntryStatus? Status = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    bool Descending = false) : IPagedQuery<CalendarEntryLongDto>
{
    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize is < 1 or > 100 ? 20 : PageSize;
}
