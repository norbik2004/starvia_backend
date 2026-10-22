using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Posts.Api.Endpoints.Posts;
using StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.RemovePostAttachment;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Posts.Api.Endpoints.PostAttachments.Commands.RemovePostAttachment;

[Authorize]
internal sealed class RemovePostAttachmentEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<RemovePostAttachmentRequest>.WithActionResult
{
    [HttpDelete($"{PostAttachmentsEndpoint.BasePath}/attachments")]
    [SwaggerOperation(Summary = "Remove post attachment", Tags = [PostAttachmentsEndpoint.Tag])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<ActionResult> HandleAsync(
        [FromBody] RemovePostAttachmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new RemovePostAttachmentCommand(request, userId);
        await dispatcher.SendAsync(command, cancellationToken);

        return NoContent();
    }
}
