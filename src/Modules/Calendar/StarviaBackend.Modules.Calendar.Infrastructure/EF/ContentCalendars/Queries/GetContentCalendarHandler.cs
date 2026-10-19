using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Queries.GetContentCalendar;
using StarviaBackend.Modules.Calendar.Core.Calendars.Exceptions;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Calendar.Infrastructure.EF.ContentCalendars.Queries;

internal sealed class GetContentCalendar(CalendarReadDbContext dbContext) : IQueryHandler<GetContentCalendarQuery, ContentCalendarLongDto>
{
    public async Task<ContentCalendarLongDto> HandleAsync(GetContentCalendarQuery query, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.ContentCalendars
            .FirstOrDefaultAsync(cc => cc.CreatedBy == query.UserId, cancellationToken);

        if(item == null)
        {
            throw new CalendarNotFoundException(Guid.Parse(query.UserId));
        }

        return new ContentCalendarLongDto(item.Id, item.Name, item.CreatedAt);
    }
}
