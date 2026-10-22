using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Domain;

namespace StarviaBackend.Modules.Calendar.Core.Calendars.Entities;

internal sealed class ContentCalendar : AuditableEntity
{
    private readonly List<CalendarEntry> _calendarEntries = [];

    public string Name { get; private set; }

    public IReadOnlyCollection<CalendarEntry> CalendarEntries => _calendarEntries.AsReadOnly();

    private ContentCalendar()
    {
    }

    public static ContentCalendar Create(string name)
    {
        return new ContentCalendar
        {
            Id = Guid.NewGuid(),
            Name = name
        };
    }

    public void UpdateName(string name)
    {
        Name = name;
    }
}
