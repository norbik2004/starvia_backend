using System;
using System.Collections.Generic;
using System.Text;

namespace StarviaBackend.Modules.Calendar.Core.Calendars.Enums;

public enum CalendarEntryStatus
{
    Scheduled,
    Processing, 
    Published,
    Failed,
    Cancelled 
}
