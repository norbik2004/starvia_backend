using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Ai.Application.Chats.Commands.AddChatMessage;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Ai.Api.Endpoints.Chats.Commands.AddChatMessage;

/// <summary>The chat id comes from the route, the prompt from the body.</summary>
internal sealed class AddChatMessageHttpRequest
{
    [FromRoute(Name = "chatId")]
    public Guid ChatId { get; init; }

    [FromBody]
    public AddChatMessageRequest Body { get; init; } = null!;
}

[Authorize]
internal sealed class AddChatMessageEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<AddChatMessageHttpRequest>.WithActionResult<AddChatMessageResult>
{
    [HttpPost($"{ChatsEndpoint.BasePath}/{{chatId:guid}}/messages")]
    [SwaggerOperation(Summary = "Add a prompt to the chat", Tags = [ChatsEndpoint.Tag])]
    [ProducesResponseType(typeof(AddChatMessageResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult<AddChatMessageResult>> HandleAsync(
        AddChatMessageHttpRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new AddChatMessageCommand(request.ChatId, request.Body, userId);
        var result = await dispatcher.SendAsync<AddChatMessageCommand, AddChatMessageResult>(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}
