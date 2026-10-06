using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Posts.Application.Posts.Queries.BrowsePosts;
using StarviaBackend.Modules.Posts.Application.Posts.Queries.GetPost;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Posts.Api.Endpoints.Posts.Queries.GetPost;

[Authorize]
internal sealed class GetPostEndpoint(
    IDispatcher dispatcher,
    IContext context)
    : EndpointBaseAsync.WithRequest<Guid>.WithActionResult<PostLongDto>
{
    [HttpGet($"{PostsEndpoint.BasePath}/{{postId:guid}}")]
    [SwaggerOperation(Summary = "Get post by Id", Tags = [PostsEndpoint.Tag])]
    [ProducesResponseType(typeof(PostLongDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<ActionResult<PostLongDto>> HandleAsync( [FromRoute] Guid postId, CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var query = new GetPostQuery(
            PostId: postId,
            UserId: userId.ToString());

        var result = await dispatcher.QueryAsync(
            query,
            cancellationToken);

        return Ok(result);
    }
}
