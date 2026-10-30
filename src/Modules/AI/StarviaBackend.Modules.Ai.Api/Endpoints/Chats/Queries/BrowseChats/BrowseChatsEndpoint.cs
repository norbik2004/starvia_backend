using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Ai.Application.Chats.Queries.BrowseChats;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Ai.Api.Endpoints.Chats.Queries.BrowseChats;

[Authorize]
internal sealed class BrowseChatsEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<BrowseChatsRequest>.WithActionResult<PagedResult<ChatDto>>
{
    [HttpGet(ChatsEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Browse my chat history", Tags = [ChatsEndpoint.Tag])]
    [ProducesResponseType(typeof(PagedResult<ChatDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult<PagedResult<ChatDto>>> HandleAsync(
        [FromQuery] BrowseChatsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var query = new BrowseChatsQuery(userId, request.Type, request.Status, request.Page, request.PageSize);
        var result = await dispatcher.QueryAsync(query, cancellationToken);

        return Ok(result);
    }
}
