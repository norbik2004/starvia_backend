using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Posts.Application.Posts.Queries.GetPost;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Posts.Api.Endpoints.Posts.Queries.GetPost;

[Authorize]
internal sealed class GetPostEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<Guid>.WithActionResult<PostLongDto>
{
    [HttpGet($"{PostsEndpoint.BasePath}/{{postId:guid}}")]
    [SwaggerOperation(Summary = "Get post by Id", Tags = [PostsEndpoint.Tag])]
    [ProducesResponseType(typeof(PostLongDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public override async Task<ActionResult<PostLongDto>> HandleAsync([FromRoute] Guid postId, CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(new GetPostQuery(postId), cancellationToken);

        return Ok(result);
    }
}
