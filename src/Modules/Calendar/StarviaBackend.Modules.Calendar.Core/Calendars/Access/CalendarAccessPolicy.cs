using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Calendar.Core.Calendars.Access;

/// <summary>
/// Everything a caller can try to do with calendars and their entries. Entries have no owner of
/// their own: the rule is applied to the owner of the calendar they belong to. Add a value here,
/// then give it a rule in <see cref="CalendarAccessPolicy"/>.
/// </summary>
internal enum CalendarAction
{
    ViewCalendar,
    UpdateCalendar,
    DeleteCalendar,
    ViewEntries,
    ManageEntries,
}

/// <summary>
/// THE place where access rules for the Calendar module are defined. Nothing else in the module
/// compares <c>CreatedBy</c> to a user id or looks at roles: handlers and queries ask
/// <c>ICalendarAccessService</c>, which asks this policy.
/// </summary>
internal static class CalendarAccessPolicy
{
    public static AccessScope ScopeFor(CalendarAction action, AccessActor actor) => action switch
    {
        CalendarAction.ViewCalendar => actor.OwnerOrAdmin(),
        CalendarAction.UpdateCalendar => actor.OwnerOrAdmin(),
        CalendarAction.DeleteCalendar => actor.OwnerOrAdmin(),
        CalendarAction.ViewEntries => actor.OwnerOrAdmin(),
        CalendarAction.ManageEntries => actor.OwnerOrAdmin(),
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "No access rule defined for this action."),
    };
}
