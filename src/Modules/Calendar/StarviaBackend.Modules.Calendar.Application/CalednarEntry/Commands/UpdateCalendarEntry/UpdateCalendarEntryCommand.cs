using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Calendar.Core.Calendars.Enums;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.UpdateCalendarEntry;

internal sealed record UpdateCalendarEntryCommand(UpdateCalendarEntryRequest request, Guid UserId) : ICommand<UpdateCalendarEntryResult>;
internal sealed record UpdateCalendarEntryResult(Guid CalendarEntryId);
internal sealed record UpdateCalendarEntryRequest(Guid CalendarEntryId, DateTimeOffset ScheduledAt);
