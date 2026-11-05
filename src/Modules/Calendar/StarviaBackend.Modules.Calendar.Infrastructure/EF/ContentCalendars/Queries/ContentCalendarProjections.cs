using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Queries.GetContentCalendar;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.ContentCalendars.Configurations.Read.Models;

namespace StarviaBackend.Modules.Calendar.Infrastructure.EF.ContentCalendars.Queries;

internal sealed record ContentCalendarRow(ContentCalendarLongDto Calendar, string OwnerId);

internal static class ContentCalendarProjections
{
    /// <summary>
    /// Projects the first matching calendar (with its entries in schedule order) in one query,
    /// together with its owner so the caller can run the access check.
    /// </summary>
    public static async Task<ContentCalendarRow?> FirstRowAsync(
        this IQueryable<ContentCalendarReadModel> calendars,
        CancellationToken cancellationToken)
    {
        var row = await calendars
            .AsNoTracking()
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.CreatedAt,
                c.CreatedBy,
                Entries = c.CalendarEntries
                    .OrderBy(e => e.ScheduledAt)
                    .ThenBy(e => e.Id)
                    .Select(e => new { e.Id, e.PostId, e.ScheduledAt, e.Status })
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        ICollection<CalendarEntryLongDto> entries = row.Entries
            .Select(e => new CalendarEntryLongDto(e.Id, e.PostId, e.ScheduledAt, e.Status))
            .ToList()
            .AsReadOnly();

        return new ContentCalendarRow(new ContentCalendarLongDto(row.Id, row.Name, row.CreatedAt, entries), row.CreatedBy);
    }
}
