using StarviaBackend.Modules.Calendar.Application.Access;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Queries.GetContentCalendar;
using StarviaBackend.Modules.Calendar.Core.Calendars.Access;
using StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Calendar.Infrastructure.EF.ContentCalendars.Queries;

internal sealed class GetContentCalendarHandler(CalendarReadDbContext dbContext, ICalendarAccessService access)
    : IQueryHandler<GetContentCalendarQuery, ContentCalendarLongDto>
{
    public async Task<ContentCalendarLongDto> HandleAsync(GetContentCalendarQuery query, CancellationToken cancellationToken = default)
    {
        var row = await dbContext.ContentCalendars
            .Where(c => c.Id == query.CalendarId)
            .FirstRowAsync(cancellationToken)
            ?? throw new CalendarNotFoundException(query.CalendarId);

        access.EnsureAllowed(CalendarAction.ViewCalendar, row.Calendar.Id, row.OwnerId);

        return row.Calendar;
    }
}

internal sealed class GetMyContentCalendarHandler(CalendarReadDbContext dbContext)
    : IQueryHandler<GetMyContentCalendarQuery, ContentCalendarLongDto>
{
    public async Task<ContentCalendarLongDto> HandleAsync(GetMyContentCalendarQuery query, CancellationToken cancellationToken = default)
    {
        var ownerId = query.UserId.ToString();

        // "My" calendar is a selector (whose calendar), not an access decision, so no policy check here.
        var row = await dbContext.ContentCalendars
            .Where(c => c.CreatedBy == ownerId)
            .FirstRowAsync(cancellationToken)
            ?? throw new CalendarNotFoundException(query.UserId);

        return row.Calendar;
    }
}
