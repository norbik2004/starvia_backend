using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Posts.Application.PostPublications.Queries.BrowsePostPublications;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Posts.Api.Endpoints.PostPublications.Queries.BrowsePostPublications;

[Authorize]
internal sealed class BrowsePostPublicationsEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<BrowsePostPublicationsQuery>.WithActionResult<IReadOnlyList<PostPublicationDto>>
{
    [HttpGet(PostPublicationsEndpoint.BasePath)]
    [SwaggerOperation(
        Summary = "Browse publications of a post",
        Description = "Newest first, optionally filtered by status and user platform. Allowed for the post's creator and admins.",
        Tags = [PostPublicationsEndpoint.Tag])]
    [ProducesResponseType(typeof(IReadOnlyList<PostPublicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<IReadOnlyList<PostPublicationDto>>> HandleAsync(
        [FromQuery] BrowsePostPublicationsQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(request, cancellationToken);
        return Ok(result);
    }
}
