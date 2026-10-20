using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.AddCalendarEntry;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Calendar.Api.Endpoints.CalendarEntries.Commands.AddCalendarEntry;

[Authorize]
internal sealed class AddCalendarEntryEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<AddCalendarEntryRequest>.WithActionResult<AddCalendarEntryResult>
{
    [HttpPost(CalendarEntriesEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Add calendar entry", Tags = [CalendarEntriesEndpoint.Tag])]
    [ProducesResponseType(typeof(AddCalendarEntryResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult<AddCalendarEntryResult>> HandleAsync(
        [FromBody] AddCalendarEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new AddCalendarEntryCommand(request, userId);
        var result = await dispatcher.SendAsync<AddCalendarEntryCommand, AddCalendarEntryResult>(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}
