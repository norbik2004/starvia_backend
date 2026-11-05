using StarviaBackend.Modules.Calendar.Application.Access;
using StarviaBackend.Modules.Calendar.Core.Calendars.Access;
using StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;
using StarviaBackend.Modules.Calendar.Core.Calendars.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.UpdateCalendarEntry;

internal sealed class UpdateCalendarEntryHandler(
    ICalendarEntryRepository calendarEntryRepository,
    ICalendarRepository calendarRepository,
    ICalendarAccessService access)
    : ICommandHandler<UpdateCalendarEntryCommand, UpdateCalendarEntryResult>
{
    public async Task<UpdateCalendarEntryResult> HandleAsync(UpdateCalendarEntryCommand command, CancellationToken cancellationToken = default)
    {
        var calendarEntryId = command.request.CalendarEntryId;
        var calendarEntry = await calendarEntryRepository.GetByIdAsync(calendarEntryId, cancellationToken)
            ?? throw new CalendarEntryNotFoundException(calendarEntryId);

        // Entries are owned by whoever owns their calendar, not by whoever happened to create them.
        var calendar = await calendarRepository.GetByIdAsync(calendarEntry.ContentCalendarId, cancellationToken)
            ?? throw new CalendarNotFoundException(calendarEntry.ContentCalendarId);

        access.EnsureAllowed(CalendarAction.ManageEntries, calendar);

        calendarEntry.UpdateScheulde(command.request.ScheduledAt);
        await calendarEntryRepository.UpdateAsync(calendarEntry, cancellationToken);

        return new UpdateCalendarEntryResult(calendarEntryId);
    }
}
