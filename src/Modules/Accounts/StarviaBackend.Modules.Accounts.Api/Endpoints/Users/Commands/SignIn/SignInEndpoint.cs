using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Accounts.Api.Endpoints.Users;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.SignIn;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Exceptions;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Accounts.Api.Endpoints.Users.Commands.SignIn;

[AllowAnonymous]
internal sealed class SignInEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<SignInCommand>.WithActionResult<JsonWebToken>
{
    [HttpPost($"{UsersEndpoint.BasePath}/sign-in")]
    [SwaggerOperation(Summary = "Sign in and receive a JWT", Tags = [UsersEndpoint.Tag])]
    [ProducesResponseType(typeof(JsonWebToken), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public override async Task<ActionResult<JsonWebToken>> HandleAsync(
        SignInCommand request,
        CancellationToken cancellationToken = default)
    {
        var token = await dispatcher.SendAsync<SignInCommand, JsonWebToken>(request, cancellationToken);
        return Ok(token);
    }
}
