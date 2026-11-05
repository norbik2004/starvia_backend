using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Ai.Application.PostGeneration.Commands.GeneratePost;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Ai.Api.Endpoints.PostGeneration.Commands.GeneratePost;

[Authorize]
internal sealed class GeneratePostEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<GeneratePostRequest>.WithActionResult<GeneratePostResult>
{
    [HttpPost($"{PostGenerationEndpoint.BasePath}/generate")]
    [SwaggerOperation(
        Summary = "Generate a post with AI",
        Description = "Writes or refines post content from the prompt for the given post id. "
                      + "A post-generation chat is created (or reused) for that post; send the same post id with the next prompt to ask for changes.",
        Tags = [PostGenerationEndpoint.Tag])]
    [ProducesResponseType(typeof(GeneratePostResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult<GeneratePostResult>> HandleAsync(
        [FromBody] GeneratePostRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new GeneratePostCommand(request, userId);
        var result = await dispatcher.SendAsync<GeneratePostCommand, GeneratePostResult>(command, cancellationToken);

        return Ok(result);
    }
}
