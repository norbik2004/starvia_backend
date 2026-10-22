using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Calendar.Core.Calendars.Enums;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.ContentCalendars.Configurations.Read.Models;

namespace StarviaBackend.Modules.Calendar.Infrastructure.EF.CalendarEntries.Configurations.Read.Models;

internal class CalendarEntryReadModel
{
    public Guid Id { get; init; }
    public Guid ContentCalendarId { get; init; }
    public Guid PostId { get; init; }
    public DateTimeOffset ScheduledAt { get; init; }
    public CalendarEntryStatus Status { get; init; }
}
