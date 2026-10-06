using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Posts.Application.Posts.Commands.AddPost;
using StarviaBackend.Modules.Posts.Application.Posts.Commands.UpdatePost;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Posts.Api.Endpoints.Posts.Commands.UpdatePost;

[Authorize]
internal sealed class UpdatePostEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<UpdatePostRequest>.WithActionResult<UpdatePostResult>
{
    [HttpPut(PostsEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Update post", Tags = [PostsEndpoint.Tag])]
    [ProducesResponseType(typeof(PagedResult<AddPostResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<UpdatePostResult>> HandleAsync(UpdatePostRequest request, CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new UpdatePostCommand(request, userId);
        var result = await dispatcher.SendAsync<UpdatePostCommand, UpdatePostResult>(command, cancellationToken);

        return Ok(result);
    }
}
