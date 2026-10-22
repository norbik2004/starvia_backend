using StarviaBackend.Modules.Calendar.Core.Calendars.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;
using StarviaBackend.Modules.Calendar.Core.Calendars.Entities;

namespace StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.AddContentCalendar;

internal sealed class AddContentCalendarHandler(ICalendarRepository calendarRepository)
    : ICommandHandler<AddContentCalendarCommand, AddContentCalendarResult>
{
    public async Task<AddContentCalendarResult> HandleAsync(AddContentCalendarCommand command, CancellationToken cancellationToken = default)
    {
        var request = command.request;

        var existingCalendar = await calendarRepository.GetByUserIdAsync(command.userId, cancellationToken);

        if(existingCalendar != null)
        {
            throw new CantCreateCalendarException(command.userId);
        }

        var calendar = ContentCalendar.Create(request.name);
        await calendarRepository.AddAsync(calendar, cancellationToken);

        return new AddContentCalendarResult(calendar.Id);
    }
}
