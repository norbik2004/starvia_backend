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
        Description = "Writes a post from the prompt. Without a chat id a new post generation chat is started; "
                      + "send the returned chat id back with the next prompt to ask for changes to the post.",
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
