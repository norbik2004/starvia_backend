using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Accounts.Api.Endpoints.Users;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.RequestPasswordReset;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Exceptions;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Accounts.Api.Endpoints.Users.Commands.RequestPasswordReset;

[AllowAnonymous]
internal sealed class RequestPasswordResetEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<RequestPasswordResetCommand>.WithActionResult
{
    [HttpPost($"{UsersEndpoint.BasePath}/forgot-password")]
    [SwaggerOperation(Summary = "Request a password reset email", Tags = [UsersEndpoint.Tag])]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public override async Task<ActionResult> HandleAsync(
        RequestPasswordResetCommand request,
        CancellationToken cancellationToken = default)
    {
        await dispatcher.SendAsync(request, cancellationToken);
        return Ok();
    }
}
