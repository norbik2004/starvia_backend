using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Posts.Application.Posts.Queries.BrowsePosts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Posts.Api.Endpoints.Posts.Queries.BrowsePosts;

[Authorize]
internal sealed class BrowsePostsEndpoint(IDispatcher dispatcher) : EndpointBaseAsync.WithRequest<BrowsePostsQuery>.WithActionResult<PagedResult<PostDto>>
{
    [HttpGet(PostsEndpoint.BasePath)]
    [SwaggerOperation(
        Summary = "Browse posts",
        Description = "Admins see all posts, other users only their own. Supports filtering by status, title text and creator, and sorting.",
        Tags = [PostsEndpoint.Tag])]
    [ProducesResponseType(typeof(PagedResult<PostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async override Task<ActionResult<PagedResult<PostDto>>> HandleAsync([FromQuery] BrowsePostsQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(request, cancellationToken);
        return Ok(result);
    }
}
