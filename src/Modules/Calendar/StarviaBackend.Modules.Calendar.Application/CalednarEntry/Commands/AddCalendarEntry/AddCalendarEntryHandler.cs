using StarviaBackend.Modules.Calendar.Core.Calendars.Entities;
using StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;
using StarviaBackend.Modules.Calendar.Core.Calendars.Repositories;
using StarviaBackend.Modules.Posts.Integrations;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.AddCalendarEntry;

internal sealed class AddCalendarEntryHandler(
    ICalendarRepository calendarRepository,
    ICalendarEntryRepository calendarEntryRepository,
    IPostsClient postsClient)
    : ICommandHandler<AddCalendarEntryCommand, AddCalendarEntryResult>
{
    public async Task<AddCalendarEntryResult> HandleAsync(
        AddCalendarEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = command.request;

        var calendar = await calendarRepository.GetByIdAsync(request.CalendarId, cancellationToken)
            ?? throw new CalendarNotFoundException(request.CalendarId);

        if (calendar.CreatedBy != command.UserId.ToString())
        {
            throw new CalendarNotOwnedException(calendar.Id);
        }

        // Throws when the post does not exist or its creator is not the requesting user.
        var posts = await postsClient.GetPostsAsync(command.UserId, [request.PostId], cancellationToken);
        var post = posts.Single();

        var calendarEntry = CalendarEntry.Create(calendar.Id, post.Id, request.ScheduledAt);
        await calendarEntryRepository.AddAsync(calendarEntry, cancellationToken);

        return new AddCalendarEntryResult(calendarEntry.Id);
    }
}
