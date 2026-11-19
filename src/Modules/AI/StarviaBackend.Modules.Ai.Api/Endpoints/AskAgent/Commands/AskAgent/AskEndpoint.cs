using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Ai.Application.AskAgent.Commands.AskAgent;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Ai.Api.Endpoints.AskAgent.Commands.AskAgent;

[Authorize]
internal sealed class AskEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<AskAgentRequest>.WithActionResult<AskAgentResult>
{
    [HttpPost($"{AskAgentEndpoint.BasePath}/ask")]
    [SwaggerOperation(
        Summary = "Ask the social media AI assistant",
        Description = "Prompts the AskAgent helper (strategy, ideas, critique). "
                      + "Omit chatId to start a new conversation; reuse the returned chatId to continue with history. "
                      + "Separate from post generation — answers are plain text, not publish-ready post JSON.",
        Tags = [AskAgentEndpoint.Tag])]
    [ProducesResponseType(typeof(AskAgentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<AskAgentResult>> HandleAsync(
        [FromBody] AskAgentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new AskAgentCommand(request, userId);
        var result = await dispatcher.SendAsync<AskAgentCommand, AskAgentResult>(command, cancellationToken);

        return Ok(result);
    }
}
