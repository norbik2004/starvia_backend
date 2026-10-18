using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.AddContentCalendar;
using StarviaBackend.Modules.Calendar.Core.Calendars.Entities;
using StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;
using StarviaBackend.Modules.Calendar.Core.Calendars.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.UpdateContentCalendar;

internal sealed class UpdateContentCalendarHandler(ICalendarRepository calendarRepository)
    : ICommandHandler<UpdateContentCalendarCommand, UpdateContentCalendarResult>
{
    public async Task<UpdateContentCalendarResult> HandleAsync(UpdateContentCalendarCommand command, CancellationToken cancellationToken = default)
    {
        var request = command.request;

        var contentCalendar = await calendarRepository.GetByIdAsync(request.contentCalendarId, cancellationToken);

        if (contentCalendar is null)
        {
            throw new CalendarNotFoundException(request.contentCalendarId);
        }
 
        if (contentCalendar.CreatedBy != command.userId.ToString())
        {
            throw new UnauthorizedAccessException();
        }

        contentCalendar.UpdateName(request.name);
        await calendarRepository.UpdateAsync(contentCalendar, cancellationToken);

        return new UpdateContentCalendarResult(contentCalendar.Id);
    }
}
