using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Posts.Api.Endpoints.Posts;
using StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.AddPostAttachment;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Posts.Api.Endpoints.PostAttachments.Commands.AddPostAttachments;

[Authorize]
internal sealed class AddPostAttachmentsEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<AddPostAttachmentsRequest>.WithActionResult<AddPostAttachmentsResult>
{
    [HttpPost($"{PostAttachmentsEndpoint.BasePath}/attachments")]
    [SwaggerOperation(Summary = "Add post attachments", Tags = [PostAttachmentsEndpoint.Tag])]
    [ProducesResponseType(typeof(AddPostAttachmentsResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<AddPostAttachmentsResult>> HandleAsync(
        [FromBody] AddPostAttachmentsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new AddPostAttachmentsCommand(request, userId);
        var result = await dispatcher.SendAsync<AddPostAttachmentsCommand, AddPostAttachmentsResult>(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}
