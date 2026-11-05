using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Queries.GetContentCalendar;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Calendar.Api.Endpoints.ContentCalendars.Queries.GetMyContentCalendar;

[Authorize]
internal sealed class GetMyContentCalendarEndpoint(
    IDispatcher dispatcher,
    IContext context)
    : EndpointBaseAsync.WithoutRequest.WithActionResult<ContentCalendarLongDto>
{
    [HttpGet($"{ContentCalendarsEndpoint.BasePath}")]
    [SwaggerOperation(Summary = "Get My Calendar", Tags = [ContentCalendarsEndpoint.Tag])]
    [ProducesResponseType(typeof(ContentCalendarLongDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult<ContentCalendarLongDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await dispatcher.QueryAsync(new GetMyContentCalendarQuery(userId), cancellationToken);

        return Ok(result);
    }
}
