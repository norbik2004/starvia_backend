using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Platforms.Application.LinkedIn.Commands.GetLinkedInAuthorizeUrl;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Platforms.Api.Endpoints.LinkedIn.Commands.GetLinkedInAuthorizeUrl;

[Authorize]
internal sealed class GetLinkedInAuthorizeUrlEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithoutRequest.WithActionResult<GetLinkedInAuthorizeUrlResult>
{
    [HttpGet($"{LinkedInEndpoint.BasePath}/authorize")]
    [SwaggerOperation(
        Summary = "Get LinkedIn OAuth authorization URL",
        Description = "Returns the LinkedIn authorization URL. Redirect the user there to connect their account.",
        Tags = [LinkedInEndpoint.Tag])]
    [ProducesResponseType(typeof(GetLinkedInAuthorizeUrlResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult<GetLinkedInAuthorizeUrlResult>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await dispatcher.SendAsync<GetLinkedInAuthorizeUrlCommand, GetLinkedInAuthorizeUrlResult>(
            new GetLinkedInAuthorizeUrlCommand(userId),
            cancellationToken);

        return Ok(result);
    }
}
