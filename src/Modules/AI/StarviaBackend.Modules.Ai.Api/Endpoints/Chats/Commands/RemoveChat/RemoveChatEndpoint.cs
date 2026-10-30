using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Ai.Application.Chats.Commands.RemoveChat;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Ai.Api.Endpoints.Chats.Commands.RemoveChat;

[Authorize]
internal sealed class RemoveChatEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<Guid>.WithActionResult
{
    [HttpDelete($"{ChatsEndpoint.BasePath}/{{chatId:guid}}")]
    [SwaggerOperation(Summary = "Remove chat together with its history", Tags = [ChatsEndpoint.Tag])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult> HandleAsync(
        [FromRoute] Guid chatId,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new RemoveChatCommand(chatId, userId);
        await dispatcher.SendAsync(command, cancellationToken);

        return NoContent();
    }
}
