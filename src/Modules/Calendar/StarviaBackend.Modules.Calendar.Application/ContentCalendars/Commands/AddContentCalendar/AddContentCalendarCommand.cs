using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.AddContentCalendar;

internal sealed record AddContentCalendarCommand(AddContentCalendarRequest request, Guid userId) : ICommand<AddContentCalendarResult>;
internal sealed record AddContentCalendarRequest(string name);
internal sealed record AddContentCalendarResult(Guid contentCalendarId);
