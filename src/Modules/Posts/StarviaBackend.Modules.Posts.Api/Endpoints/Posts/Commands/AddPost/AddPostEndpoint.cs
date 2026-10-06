using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Posts.Application.Posts.Commands.AddPost;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Posts.Api.Endpoints.Posts.Commands.AddPost;

internal sealed class AddPostEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<AddPostRequest>.WithActionResult<AddPostResult>
{
    [HttpPost(PostsEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Add post", Tags = [PostsEndpoint.Tag])]
    [ProducesResponseType(typeof(PagedResult<AddPostResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<AddPostResult>> HandleAsync([FromBody] AddPostRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new AddPostCommand(request, userId);
        var result = await dispatcher.SendAsync<AddPostCommand, AddPostResult>(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}
