using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Posts.Application.PostAttachments.Queries.BrowsePostAttachments;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Posts.Api.Endpoints.PostAttachments.Queries.BrowsePostAttachments;

[Authorize]
internal sealed class BrowsePostAttachmentsEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<Guid>.WithActionResult<IReadOnlyList<PostAttachmentDto>>
{
    [HttpGet(PostAttachmentsEndpoint.BasePath)]
    [SwaggerOperation(
        Summary = "Browse attachments of a post",
        Description = "Returns the attachments in display order. Allowed for the post's creator and admins.",
        Tags = [PostAttachmentsEndpoint.Tag])]
    [ProducesResponseType(typeof(IReadOnlyList<PostAttachmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<IReadOnlyList<PostAttachmentDto>>> HandleAsync(
        [FromQuery] Guid postId,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(new BrowsePostAttachmentsQuery(postId), cancellationToken);
        return Ok(result);
    }
}
