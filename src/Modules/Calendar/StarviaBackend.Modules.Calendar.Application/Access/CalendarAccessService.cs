using StarviaBackend.Modules.Calendar.Core.Calendars.Access;
using StarviaBackend.Modules.Calendar.Core.Calendars.Entities;
using StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Contexts;

namespace StarviaBackend.Modules.Calendar.Application.Access;

/// <summary>
/// How handlers and queries consult <see cref="CalendarAccessPolicy"/> for the caller of the
/// current request. Holds no rules itself.
/// </summary>
internal interface ICalendarAccessService
{
    /// <summary>The calendars the current caller may use for <paramref name="action"/>; apply it as a filter when listing.</summary>
    AccessScope ScopeFor(CalendarAction action);

    /// <summary>Throws <see cref="CalendarAccessDeniedException"/> unless the caller may do <paramref name="action"/> on the calendar.</summary>
    void EnsureAllowed(CalendarAction action, Guid calendarId, string? calendarOwnerId);

    void EnsureAllowed(CalendarAction action, ContentCalendar calendar) =>
        EnsureAllowed(action, calendar.Id, calendar.CreatedBy);
}

internal sealed class CalendarAccessService(IContext context) : ICalendarAccessService
{
    public AccessScope ScopeFor(CalendarAction action) =>
        CalendarAccessPolicy.ScopeFor(action, AccessActor.From(context.Identity));

    public void EnsureAllowed(CalendarAction action, Guid calendarId, string? calendarOwnerId)
    {
        if (!ScopeFor(action).Allows(calendarOwnerId))
        {
            throw new CalendarAccessDeniedException(calendarId, action);
        }
    }
}
