using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.CalendarEntries.Configurations.Read.Models;

namespace StarviaBackend.Modules.Calendar.Infrastructure.EF.ContentCalendars.Configurations.Read.Models;

internal class ContentCalendarReadModel
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; }
    public ICollection<CalendarEntryReadModel> CalendarEntries { get; init; } = [];
}
