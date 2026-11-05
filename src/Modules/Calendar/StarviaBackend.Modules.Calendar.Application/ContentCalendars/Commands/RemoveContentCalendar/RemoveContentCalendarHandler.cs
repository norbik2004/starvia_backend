using StarviaBackend.Modules.Calendar.Application.Access;
using StarviaBackend.Modules.Calendar.Core.Calendars.Access;
using StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;
using StarviaBackend.Modules.Calendar.Core.Calendars.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.RemoveContentCalendar;

internal sealed class RemoveContentCalendarHandler(ICalendarRepository calendarRepository, ICalendarAccessService access)
    : ICommandHandler<RemoveContentCalendarCommand>
{
    public async Task HandleAsync(RemoveContentCalendarCommand command, CancellationToken cancellationToken = default)
    {
        var request = command.request;

        var contentCalendar = await calendarRepository.GetByIdAsync(request.contentCalendarId, cancellationToken)
            ?? throw new CalendarNotFoundException(request.contentCalendarId);

        access.EnsureAllowed(CalendarAction.DeleteCalendar, contentCalendar);

        await calendarRepository.RemoveAsync(contentCalendar, cancellationToken);
    }
}
