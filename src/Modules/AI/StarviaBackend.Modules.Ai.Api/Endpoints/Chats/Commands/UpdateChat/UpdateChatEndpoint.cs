using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Ai.Application.Chats.Commands.UpdateChat;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Ai.Api.Endpoints.Chats.Commands.UpdateChat;

/// <summary>The chat id comes from the route, the new values from the body.</summary>
internal sealed class UpdateChatHttpRequest
{
    [FromRoute(Name = "chatId")]
    public Guid ChatId { get; init; }

    [FromBody]
    public UpdateChatRequest Body { get; init; } = null!;
}

[Authorize]
internal sealed class UpdateChatEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<UpdateChatHttpRequest>.WithActionResult
{
    [HttpPut($"{ChatsEndpoint.BasePath}/{{chatId:guid}}")]
    [SwaggerOperation(Summary = "Update chat (rename, archive or restore)", Tags = [ChatsEndpoint.Tag])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult> HandleAsync(
        UpdateChatHttpRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new UpdateChatCommand(request.ChatId, request.Body, userId);
        await dispatcher.SendAsync(command, cancellationToken);

        return NoContent();
    }
}
