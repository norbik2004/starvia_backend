using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Platforms.Application.LinkedIn.Commands.ConnectLinkedInAccount;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Platforms.Api.Endpoints.LinkedIn.Commands.ConnectLinkedInAccount;

/// <summary>
/// LinkedIn redirects the browser here after consent. AllowAnonymous because the redirect has no JWT;
/// the OAuth <c>state</c> carries the user id.
/// </summary>
[AllowAnonymous]
internal sealed class ConnectLinkedInAccountEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<ConnectLinkedInAccountRequest>.WithActionResult
{
    [HttpGet($"{LinkedInEndpoint.BasePath}/callback")]
    [SwaggerOperation(
        Summary = "LinkedIn OAuth callback",
        Description = "Exchanges the authorization code for a token, stores the connected account, and redirects to the frontend.",
        Tags = [LinkedInEndpoint.Tag])]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public override async Task<ActionResult> HandleAsync(
        [FromQuery] ConnectLinkedInAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Error) is false)
        {
            return BadRequest(request.ErrorDescription ?? request.Error);
        }

        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.State))
        {
            return BadRequest("Missing LinkedIn authorization code or state.");
        }

        var result = await dispatcher.SendAsync<ConnectLinkedInAccountCommand, ConnectLinkedInAccountResult>(
            new ConnectLinkedInAccountCommand(request.Code, request.State),
            cancellationToken);

        return Redirect(result.SuccessRedirectUrl);
    }
}

public sealed record ConnectLinkedInAccountRequest(
    string? Code,
    string? State,
    string? Error,
    string? ErrorDescription);
