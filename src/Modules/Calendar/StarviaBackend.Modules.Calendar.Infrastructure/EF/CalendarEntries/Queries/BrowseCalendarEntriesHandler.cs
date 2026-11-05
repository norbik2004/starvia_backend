using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Calendar.Application.Access;
using StarviaBackend.Modules.Calendar.Application.CalednarEntry.Queries.BrowseCalendarEntries;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Queries.GetContentCalendar;
using StarviaBackend.Modules.Calendar.Core.Calendars.Access;
using StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Calendar.Infrastructure.EF.CalendarEntries.Queries;

internal sealed class BrowseCalendarEntriesHandler(CalendarReadDbContext dbContext, ICalendarAccessService access)
    : IQueryHandler<BrowseCalendarEntriesQuery, PagedResult<CalendarEntryLongDto>>
{
    public async Task<PagedResult<CalendarEntryLongDto>> HandleAsync(
        BrowseCalendarEntriesQuery query,
        CancellationToken cancellationToken = default)
    {
        var calendar = await dbContext.ContentCalendars
            .AsNoTracking()
            .Where(c => c.Id == query.CalendarId)
            .Select(c => new { c.CreatedBy })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new CalendarNotFoundException(query.CalendarId);

        access.EnsureAllowed(CalendarAction.ViewEntries, query.CalendarId, calendar.CreatedBy);

        var entries = dbContext.CalendarEntries
            .AsNoTracking()
            .Where(e => e.ContentCalendarId == query.CalendarId);

        if (query.Status is { } status)
        {
            entries = entries.Where(e => e.Status == status);
        }

        if (query.From is { } from)
        {
            entries = entries.Where(e => e.ScheduledAt >= from);
        }

        if (query.To is { } to)
        {
            entries = entries.Where(e => e.ScheduledAt < to);
        }

        var total = await entries.LongCountAsync(cancellationToken);

        // Also guards against Skip() overflowing for absurdly large page numbers.
        var skip = (long)(query.Page - 1) * query.PageSize;
        if (skip >= total)
        {
            return new PagedResult<CalendarEntryLongDto>([], query.Page, query.PageSize, total);
        }

        // Id is the tie-breaker so paging stays stable when several entries share a schedule.
        var ordered = query.Descending
            ? entries.OrderByDescending(e => e.ScheduledAt).ThenByDescending(e => e.Id)
            : entries.OrderBy(e => e.ScheduledAt).ThenBy(e => e.Id);

        var items = await ordered
            .Skip((int)skip)
            .Take(query.PageSize)
            .Select(e => new CalendarEntryLongDto(e.Id, e.PostId, e.ScheduledAt, e.Status))
            .ToListAsync(cancellationToken);

        return new PagedResult<CalendarEntryLongDto>(items, query.Page, query.PageSize, total);
    }
}
