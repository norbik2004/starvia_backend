using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Ai.Application.Chats.Commands.AddChat;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Ai.Api.Endpoints.Chats.Commands.AddChat;

[Authorize]
internal sealed class AddChatEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<AddChatRequest>.WithActionResult<AddChatResult>
{
    [HttpPost(ChatsEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Add chat", Tags = [ChatsEndpoint.Tag])]
    [ProducesResponseType(typeof(AddChatResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult<AddChatResult>> HandleAsync(
        [FromBody] AddChatRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new AddChatCommand(request, userId);
        var result = await dispatcher.SendAsync<AddChatCommand, AddChatResult>(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}
