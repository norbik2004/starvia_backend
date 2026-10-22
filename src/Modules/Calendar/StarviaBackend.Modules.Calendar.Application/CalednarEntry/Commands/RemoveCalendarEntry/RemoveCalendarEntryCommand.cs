using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.RemoveCalendarEntry;

internal sealed record RemoveCalendarEntryCommand(RemoveCalendarEntryRequest request, Guid UserId) : ICommand;
internal sealed record RemoveCalendarEntryRequest(Guid CalendarEntryId);
