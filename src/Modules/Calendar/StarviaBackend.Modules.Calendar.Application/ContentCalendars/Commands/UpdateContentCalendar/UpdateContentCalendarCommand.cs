using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.UpdateContentCalendar;

internal sealed record UpdateContentCalendarCommand(UpdateContentCalendarRequest request, Guid userId) : ICommand<UpdateContentCalendarResult>;
internal sealed record UpdateContentCalendarRequest(Guid contentCalendarId, string name);
internal sealed record UpdateContentCalendarResult(Guid contentCalendarId);
