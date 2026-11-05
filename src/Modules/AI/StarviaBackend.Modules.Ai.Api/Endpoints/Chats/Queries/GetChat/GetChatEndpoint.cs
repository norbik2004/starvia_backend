using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Ai.Application.Chats.Queries.GetChat;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Ai.Api.Endpoints.Chats.Queries.GetChat;

[Authorize]
internal sealed class GetChatEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<Guid>.WithActionResult<ChatLongDto>
{
    [HttpGet($"{ChatsEndpoint.BasePath}/{{chatId:guid}}")]
    [SwaggerOperation(Summary = "Get chat with its history", Tags = [ChatsEndpoint.Tag])]
    [ProducesResponseType(typeof(ChatLongDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<ChatLongDto>> HandleAsync(
        [FromRoute] Guid chatId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetChatQuery(chatId);
        var result = await dispatcher.QueryAsync(query, cancellationToken);

        return Ok(result);
    }
}
