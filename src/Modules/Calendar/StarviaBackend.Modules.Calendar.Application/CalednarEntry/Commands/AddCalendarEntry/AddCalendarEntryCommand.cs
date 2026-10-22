using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Calendar.Core.Calendars.Enums;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.AddCalendarEntry;

internal sealed record AddCalendarEntryCommand(AddCalendarEntryRequest request, Guid UserId) : ICommand<AddCalendarEntryResult>;
internal sealed record AddCalendarEntryResult(Guid CalendarEntryId);
internal sealed record AddCalendarEntryRequest(Guid CalendarId, Guid PostId, DateTimeOffset ScheduledAt);
