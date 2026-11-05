using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Queries.GetContentCalendar;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Calendar.Api.Endpoints.ContentCalendars.Queries.GetContentCalendar;

[Authorize]
internal sealed class GetContentCalendarEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<Guid>.WithActionResult<ContentCalendarLongDto>
{
    [HttpGet($"{ContentCalendarsEndpoint.BasePath}/{{calendarId:guid}}")]
    [SwaggerOperation(
        Summary = "Get calendar by Id",
        Description = "Allowed for the calendar's creator and admins.",
        Tags = [ContentCalendarsEndpoint.Tag])]
    [ProducesResponseType(typeof(ContentCalendarLongDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<ContentCalendarLongDto>> HandleAsync(
        [FromRoute] Guid calendarId,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(new GetContentCalendarQuery(calendarId), cancellationToken);
        return Ok(result);
    }
}
