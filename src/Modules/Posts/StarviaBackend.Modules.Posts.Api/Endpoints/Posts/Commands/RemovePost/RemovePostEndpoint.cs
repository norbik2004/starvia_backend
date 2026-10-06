using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Posts.Application.Posts.Commands.AddPost;
using StarviaBackend.Modules.Posts.Application.Posts.Commands.RemovePost;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Posts.Api.Endpoints.Posts.Commands.RemovePost;

[Authorize]
internal sealed class RemovePostEndpoint(IDispatcher dispatcher, IContext context) : EndpointBaseAsync.WithRequest<RemovePostRequest>.WithActionResult
{
    [HttpDelete(PostsEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Delete post", Tags = [PostsEndpoint.Tag])]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult> HandleAsync( [FromBody] RemovePostRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new RemovePostCommand(request, userId);
        await dispatcher.SendAsync(command, cancellationToken);

        return NoContent();
    }
}
