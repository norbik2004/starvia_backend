using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Calendar.Api.Endpoints;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.AddContentCalendar;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.RemoveContentCalendar;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Calendar.Api.Endpoints.ContentCalendars.Commands.RemoveContentCalendar;

internal sealed class RemoveContentCalendarEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<RemoveContentCalendarRequest>.WithActionResult
{
    [HttpDelete(ContentCalendarsEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Remove Content Calendar", Tags = [ContentCalendarsEndpoint.Tag])]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult> HandleAsync([FromBody] RemoveContentCalendarRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new RemoveContentCalendarCommand(request, userId);
        await dispatcher.SendAsync(command, cancellationToken);

        return StatusCode(StatusCodes.Status204NoContent);
    }
}
