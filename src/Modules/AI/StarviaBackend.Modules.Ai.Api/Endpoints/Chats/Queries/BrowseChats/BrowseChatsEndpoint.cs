using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Ai.Application.Chats.Queries.BrowseChats;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Ai.Api.Endpoints.Chats.Queries.BrowseChats;

[Authorize]
internal sealed class BrowseChatsEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<BrowseChatsQuery>.WithActionResult<PagedResult<ChatDto>>
{
    [HttpGet(ChatsEndpoint.BasePath)]
    [SwaggerOperation(
        Summary = "Browse my chat history",
        Description = "Chats are private: everybody, admins included, sees only their own. Supports filtering by type, status, title text and linked post, and sorting.",
        Tags = [ChatsEndpoint.Tag])]
    [ProducesResponseType(typeof(PagedResult<ChatDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult<PagedResult<ChatDto>>> HandleAsync(
        [FromQuery] BrowseChatsQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(request, cancellationToken);

        return Ok(result);
    }
}
