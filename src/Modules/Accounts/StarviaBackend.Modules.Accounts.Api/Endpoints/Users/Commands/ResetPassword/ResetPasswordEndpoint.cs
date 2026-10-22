using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Accounts.Api.Endpoints.Users;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.ResetPassword;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Exceptions;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Accounts.Api.Endpoints.Users.Commands.ResetPassword;

[AllowAnonymous]
internal sealed class ResetPasswordEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<ResetPasswordCommand>.WithActionResult
{
    [HttpPost($"{UsersEndpoint.BasePath}/reset-password")]
    [SwaggerOperation(Summary = "Reset a password with the mailed code", Tags = [UsersEndpoint.Tag])]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public override async Task<ActionResult> HandleAsync(
        ResetPasswordCommand request,
        CancellationToken cancellationToken = default)
    {
        await dispatcher.SendAsync(request, cancellationToken);
        return Ok();
    }
}
