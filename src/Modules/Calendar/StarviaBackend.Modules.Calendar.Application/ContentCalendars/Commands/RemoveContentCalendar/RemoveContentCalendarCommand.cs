using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.RemoveContentCalendar;

internal sealed record RemoveContentCalendarRequest(Guid contentCalendarId);
internal sealed record RemoveContentCalendarCommand(RemoveContentCalendarRequest request, Guid UserId) : ICommand;
