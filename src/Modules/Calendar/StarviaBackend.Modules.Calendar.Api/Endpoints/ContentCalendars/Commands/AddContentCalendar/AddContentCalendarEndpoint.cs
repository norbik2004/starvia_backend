using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Calendar.Api.Endpoints;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.AddContentCalendar;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Calendar.Api.Endpoints.ContentCalendars.Commands.AddContentCalendar;

internal sealed class AddContentCalendarEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<AddContentCalendarRequest>.WithActionResult<AddContentCalendarResult>
{
    [HttpPost(ContentCalendarsEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Add Content Calendar", Tags = [ContentCalendarsEndpoint.Tag])]
    [ProducesResponseType(typeof(PagedResult<AddContentCalendarResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<AddContentCalendarResult>> HandleAsync([FromBody] AddContentCalendarRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new AddContentCalendarCommand(request, userId);
        var result = await dispatcher.SendAsync<AddContentCalendarCommand, AddContentCalendarResult>(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}
