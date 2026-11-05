using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Posts.Application.PostPublications.Queries.BrowsePostPublications;
using StarviaBackend.Modules.Posts.Application.PostPublications.Queries.GetPostPublication;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Posts.Api.Endpoints.PostPublications.Queries.GetPostPublication;

[Authorize]
internal sealed class GetPostPublicationEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<Guid>.WithActionResult<PostPublicationDto>
{
    [HttpGet($"{PostPublicationsEndpoint.BasePath}/{{postPublicationId:guid}}")]
    [SwaggerOperation(Summary = "Get post publication by Id", Tags = [PostPublicationsEndpoint.Tag])]
    [ProducesResponseType(typeof(PostPublicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<PostPublicationDto>> HandleAsync(
        [FromRoute] Guid postPublicationId,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(new GetPostPublicationQuery(postPublicationId), cancellationToken);
        return Ok(result);
    }
}
