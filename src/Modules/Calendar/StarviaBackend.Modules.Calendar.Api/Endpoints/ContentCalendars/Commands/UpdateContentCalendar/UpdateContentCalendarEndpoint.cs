using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Calendar.Api.Endpoints;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.AddContentCalendar;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.RemoveContentCalendar;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.UpdateContentCalendar;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Calendar.Api.Endpoints.ContentCalendars.Commands.UpdateContentCalendar;

internal sealed class UpdateContentCalendarEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<UpdateContentCalendarRequest>.WithActionResult<UpdateContentCalendarResult>
{
    [HttpPut(ContentCalendarsEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Update Content Calendar", Tags = [ContentCalendarsEndpoint.Tag])]
    [ProducesResponseType(typeof(ActionResult<UpdateContentCalendarResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<UpdateContentCalendarResult>> HandleAsync([FromBody] UpdateContentCalendarRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new UpdateContentCalendarCommand(request, userId);
        var result = await dispatcher.SendAsync<UpdateContentCalendarCommand, UpdateContentCalendarResult>(command, cancellationToken);

        return StatusCode(StatusCodes.Status200OK, result);
    }
}
