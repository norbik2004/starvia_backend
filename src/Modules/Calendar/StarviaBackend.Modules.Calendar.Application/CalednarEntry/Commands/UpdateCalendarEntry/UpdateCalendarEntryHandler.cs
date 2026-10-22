using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;
using StarviaBackend.Modules.Calendar.Core.Calendars.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.UpdateCalendarEntry;

internal sealed class UpdateCalendarEntryHandler(ICalendarEntryRepository calendarEntryRepository)
    : ICommandHandler<UpdateCalendarEntryCommand, UpdateCalendarEntryResult>
{
    public async Task<UpdateCalendarEntryResult> HandleAsync(UpdateCalendarEntryCommand command, CancellationToken cancellationToken = default)
    {
        var calendarEntryId = command.request.CalendarEntryId;
        var calendarEntry = await calendarEntryRepository.GetByIdAsync(calendarEntryId, cancellationToken);

        if(calendarEntry is null)
        {
            throw new CalendarEntryNotFoundException(calendarEntryId);
        }

        if(calendarEntry.CreatedBy != command.UserId.ToString())
        {
            throw new UnauthorizedAccessException("You cannot update this entry");
        }

        calendarEntry.UpdateScheulde(command.request.ScheduledAt);
        await calendarEntryRepository.UpdateAsync(calendarEntry, cancellationToken);

        return new UpdateCalendarEntryResult(calendarEntryId);
    }
}
