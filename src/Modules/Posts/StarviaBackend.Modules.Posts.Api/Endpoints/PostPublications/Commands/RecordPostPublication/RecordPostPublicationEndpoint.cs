using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Posts.Application.PostPublications.Commands.RecordPostPublication;
using StarviaBackend.Modules.Posts.Application.PostPublications.Queries.BrowsePostPublications;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Posts.Api.Endpoints.PostPublications.Commands.RecordPostPublication;

[Authorize]
internal sealed class RecordPostPublicationEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<RecordPostPublicationRequest>.WithActionResult<PostPublicationDto>
{
    [HttpPost(PostPublicationsEndpoint.BasePath)]
    [SwaggerOperation(
        Summary = "Record a post publication",
        Description = "Persists a PostPublication after a successful publish to an external platform (e.g. LinkedIn).",
        Tags = [PostPublicationsEndpoint.Tag])]
    [ProducesResponseType(typeof(PostPublicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<PostPublicationDto>> HandleAsync(
        [FromBody] RecordPostPublicationRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.SendAsync<RecordPostPublicationCommand, PostPublicationDto>(
            new RecordPostPublicationCommand(request),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}
