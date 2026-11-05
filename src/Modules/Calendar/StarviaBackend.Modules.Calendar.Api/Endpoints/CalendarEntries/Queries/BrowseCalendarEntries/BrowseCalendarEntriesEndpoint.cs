using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Calendar.Application.CalednarEntry.Queries.BrowseCalendarEntries;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Queries.GetContentCalendar;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Calendar.Api.Endpoints.CalendarEntries.Queries.BrowseCalendarEntries;

[Authorize]
internal sealed class BrowseCalendarEntriesEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<BrowseCalendarEntriesQuery>.WithActionResult<PagedResult<CalendarEntryLongDto>>
{
    [HttpGet(CalendarEntriesEndpoint.BasePath)]
    [SwaggerOperation(
        Summary = "Browse entries of a calendar",
        Description = "Soonest first. Filter by status and schedule range. Allowed for the calendar's creator and admins.",
        Tags = [CalendarEntriesEndpoint.Tag])]
    [ProducesResponseType(typeof(PagedResult<CalendarEntryLongDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<PagedResult<CalendarEntryLongDto>>> HandleAsync(
        [FromQuery] BrowseCalendarEntriesQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(request, cancellationToken);
        return Ok(result);
    }
}
