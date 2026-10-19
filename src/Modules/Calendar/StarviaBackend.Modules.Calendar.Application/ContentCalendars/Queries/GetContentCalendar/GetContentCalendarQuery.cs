using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Calendar.Application.ContentCalendars.Queries.GetContentCalendar;

internal sealed record GetContentCalendarQuery(string UserId) : IQuery<ContentCalendarLongDto>;

public sealed record ContentCalendarLongDto(Guid Id, string Name, DateTime CreatedAt);
