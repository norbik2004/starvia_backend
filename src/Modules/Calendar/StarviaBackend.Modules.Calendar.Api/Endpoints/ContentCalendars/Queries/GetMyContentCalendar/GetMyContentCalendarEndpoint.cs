using System;
using System.Collections.Generic;
using System.Text;
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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<ActionResult<ContentCalendarLongDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var query = new GetContentCalendarQuery(
            UserId: userId.ToString());

        var result = await dispatcher.QueryAsync(
            query,
            cancellationToken);

        return Ok(result);
    }
}
