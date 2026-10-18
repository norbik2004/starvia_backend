using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Calendar.Core.Calendars.Enums;
using StarviaBackend.Shared.Abstractions.Domain;

namespace StarviaBackend.Modules.Calendar.Core.Calendars.Entities;

internal class CalendarEntry : AuditableEntity
{
    public Guid ContentCalendarId { get; private set; }
    public ContentCalendar Calendar { get; private set; }
    public Guid PostId { get; private set; }

    public DateTimeOffset ScheduledAt { get; private set; }
    public CalendarEntryStatus Status { get; private set; }

    private CalendarEntry()
    {
    }

    public static CalendarEntry Create(Guid calendarId, Guid postId, DateTimeOffset scheduledAt)
    {
        return new CalendarEntry
        {
            Id = Guid.NewGuid(),
            ContentCalendarId = calendarId,
            PostId = postId,
            ScheduledAt = scheduledAt,
            Status = CalendarEntryStatus.Scheduled
        };
    }
}
