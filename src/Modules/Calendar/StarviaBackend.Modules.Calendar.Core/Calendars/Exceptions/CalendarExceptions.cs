using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;

internal sealed class CalendarNotFoundException(Guid calendarId)
    : BusinessException($"Calendar with {calendarId} was not found")
{
    public override string Code => "calendar_not_found";
}


internal sealed class CalendarNotOwnedException(Guid calendarId)
    : BusinessException($"User is not the owner of calendar '{calendarId}'")
{
    public override string Code => "calendar_not_owned";
}

internal sealed class CantCreateCalendarException(Guid userId)
    : BusinessException($"User {userId} has arleady created a calendar")
{
    public override string Code => "calendar_cannot_be_created";
}
