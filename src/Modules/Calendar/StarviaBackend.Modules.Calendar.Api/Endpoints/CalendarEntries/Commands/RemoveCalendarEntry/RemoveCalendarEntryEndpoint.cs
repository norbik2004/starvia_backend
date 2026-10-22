using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.AddCalendarEntry;
using StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.RemoveCalendarEntry;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Calendar.Api.Endpoints.CalendarEntries.Commands.RemoveCalendarEntry;

[Authorize]
internal sealed class RemoveCalendarEntryEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<RemoveCalendarEntryRequest>.WithActionResult
{
    [HttpDelete(CalendarEntriesEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Remove calendar entry", Tags = [CalendarEntriesEndpoint.Tag])]
    [ProducesResponseType(typeof(AddCalendarEntryResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult> HandleAsync(
        [FromBody] RemoveCalendarEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new RemoveCalendarEntryCommand(request, userId);
        await dispatcher.SendAsync(command, cancellationToken);

        return StatusCode(StatusCodes.Status204NoContent);
    }
}
