using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;
using StarviaBackend.Modules.Calendar.Core.Calendars.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.RemoveContentCalendar;

internal sealed class RemoveContentCalendarHandler(ICalendarRepository calendarRepository) : ICommandHandler<RemoveContentCalendarCommand>
{
    public async Task HandleAsync(RemoveContentCalendarCommand command, CancellationToken cancellationToken = default)
    {
        var request = command.request;

        var contentCalendar = await calendarRepository.GetByIdAsync(request.contentCalendarId, cancellationToken);

        if (contentCalendar is null)
        {
            throw new CalendarNotFoundException(request.contentCalendarId);
        }

        if (contentCalendar.CreatedBy != command.UserId.ToString())
        {
            throw new UnauthorizedAccessException();
        }

        await calendarRepository.RemoveAsync(contentCalendar, cancellationToken);
    }
}
