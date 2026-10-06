using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Posts.Application.Posts.Queries.BrowsePosts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Posts.Api.Endpoints.Posts.Queries.BrowsePosts;

[Authorize(Roles = PostsEndpoint.AdminRole)]
internal sealed class BrowsePostsEndpoint(IDispatcher dispatcher) : EndpointBaseAsync.WithRequest<BrowsePostsQuery>.WithActionResult<PagedResult<PostDto>>
{
    [HttpGet(PostsEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Browse posts (admin only)", Tags = [PostsEndpoint.Tag])]
    [ProducesResponseType(typeof(ActionResult<PagedResult<PostDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async override Task<ActionResult<PagedResult<PostDto>>> HandleAsync( [FromQuery] BrowsePostsQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(request, cancellationToken);
        return Ok(result);
    }
}
