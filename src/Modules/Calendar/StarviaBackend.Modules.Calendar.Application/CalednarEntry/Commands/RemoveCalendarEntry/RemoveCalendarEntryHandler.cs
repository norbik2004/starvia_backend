using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;
using StarviaBackend.Modules.Calendar.Core.Calendars.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.RemoveCalendarEntry;

internal sealed class RemoveCalendarEntryHandler(ICalendarEntryRepository calendarEntryRepository)
    : ICommandHandler<RemoveCalendarEntryCommand>
{
    public async Task HandleAsync(RemoveCalendarEntryCommand command, CancellationToken cancellationToken = default)
    {
        var calendarEntryId = command.request.CalendarEntryId;
        var calendarEntry = await calendarEntryRepository.GetByIdAsync(calendarEntryId, cancellationToken);

        if (calendarEntry is null)
        {
            throw new CalendarEntryNotFoundException(calendarEntryId);
        }

        if (calendarEntry.CreatedBy != command.UserId.ToString())
        {
            throw new UnauthorizedAccessException("You cannot remove this entry");
        }

        await calendarEntryRepository.RemoveAsync(calendarEntry, cancellationToken);
    }
}
