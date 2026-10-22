using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.AddCalendarEntry;
using StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.RemoveCalendarEntry;
using StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.UpdateCalendarEntry;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Calendar.Api.Endpoints.CalendarEntries.Commands.UpdateCalendarEntry;

[Authorize]
internal sealed class UpdateCalendarEntryEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<UpdateCalendarEntryRequest>.WithActionResult<UpdateCalendarEntryResult>
{
    [HttpPut(CalendarEntriesEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Update calendar entry", Tags = [CalendarEntriesEndpoint.Tag])]
    [ProducesResponseType(typeof(AddCalendarEntryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult<UpdateCalendarEntryResult>> HandleAsync(
        [FromBody] UpdateCalendarEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new UpdateCalendarEntryCommand(request, userId);
        var result = await dispatcher.SendAsync<UpdateCalendarEntryCommand, UpdateCalendarEntryResult>(command, cancellationToken);

        return StatusCode(StatusCodes.Status200OK, result);
    }
}
