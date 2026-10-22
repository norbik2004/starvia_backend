using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;

internal sealed class CalendarEntryNotFoundException(Guid calendarEntryId)
    : BusinessException($"Calendar entry with {calendarEntryId} was not found")
{
    public override string Code => "calendar_entry_not_found";
}
