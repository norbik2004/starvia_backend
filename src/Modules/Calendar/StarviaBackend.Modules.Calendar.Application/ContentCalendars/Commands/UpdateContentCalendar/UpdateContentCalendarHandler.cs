using StarviaBackend.Modules.Calendar.Application.Access;
using StarviaBackend.Modules.Calendar.Core.Calendars.Access;
using StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;
using StarviaBackend.Modules.Calendar.Core.Calendars.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.UpdateContentCalendar;

internal sealed class UpdateContentCalendarHandler(ICalendarRepository calendarRepository, ICalendarAccessService access)
    : ICommandHandler<UpdateContentCalendarCommand, UpdateContentCalendarResult>
{
    public async Task<UpdateContentCalendarResult> HandleAsync(UpdateContentCalendarCommand command, CancellationToken cancellationToken = default)
    {
        var request = command.request;

        var contentCalendar = await calendarRepository.GetByIdAsync(request.contentCalendarId, cancellationToken)
            ?? throw new CalendarNotFoundException(request.contentCalendarId);

        access.EnsureAllowed(CalendarAction.UpdateCalendar, contentCalendar);

        contentCalendar.UpdateName(request.name);
        await calendarRepository.UpdateAsync(contentCalendar, cancellationToken);

        return new UpdateContentCalendarResult(contentCalendar.Id);
    }
}
