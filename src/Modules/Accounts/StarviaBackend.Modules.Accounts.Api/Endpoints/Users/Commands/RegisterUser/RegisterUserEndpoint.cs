using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Accounts.Api.Endpoints.Users;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.RegisterUser;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Exceptions;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Accounts.Api.Endpoints.Users.Commands.RegisterUser;

[AllowAnonymous]
internal sealed class RegisterUserEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<RegisterUserCommand>.WithActionResult<RegisterUserResult>
{
    [HttpPost($"{UsersEndpoint.BasePath}/register")]
    [SwaggerOperation(Summary = "Register a new account", Tags = [UsersEndpoint.Tag])]
    [ProducesResponseType(typeof(RegisterUserResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public override async Task<ActionResult<RegisterUserResult>> HandleAsync(
        RegisterUserCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.SendAsync<RegisterUserCommand, RegisterUserResult>(request, cancellationToken);
        return Ok(result);
    }
}
