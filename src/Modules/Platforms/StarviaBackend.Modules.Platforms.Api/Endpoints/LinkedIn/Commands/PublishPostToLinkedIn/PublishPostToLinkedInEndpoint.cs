using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Platforms.Application.LinkedIn.Commands.PublishPostToLinkedIn;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Platforms.Api.Endpoints.LinkedIn.Commands.PublishPostToLinkedIn;

[Authorize]
internal sealed class PublishPostToLinkedInEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<PublishPostToLinkedInRequest>.WithActionResult<PublishPostToLinkedInResult>
{
    [HttpPost($"{LinkedInEndpoint.BasePath}/publish")]
    [SwaggerOperation(
        Summary = "Publish a post to LinkedIn",
        Description = "Publishes the post to LinkedIn as text, article/URL (articleUrl), or images when attachments exist, then records a PostPublication.",
        Tags = [LinkedInEndpoint.Tag])]
    [ProducesResponseType(typeof(PublishPostToLinkedInResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<PublishPostToLinkedInResult>> HandleAsync(
        [FromBody] PublishPostToLinkedInRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await dispatcher.SendAsync<PublishPostToLinkedInCommand, PublishPostToLinkedInResult>(
            new PublishPostToLinkedInCommand(request, userId),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}
