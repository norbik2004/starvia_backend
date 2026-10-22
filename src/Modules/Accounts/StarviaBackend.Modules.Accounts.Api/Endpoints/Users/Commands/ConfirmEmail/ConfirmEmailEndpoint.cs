using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Accounts.Api.Endpoints.Users;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.ConfirmEmail;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Exceptions;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Accounts.Api.Endpoints.Users.Commands.ConfirmEmail;

[AllowAnonymous]
internal sealed class ConfirmEmailEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<ConfirmEmailCommand>.WithActionResult
{
    [HttpPost($"{UsersEndpoint.BasePath}/confirm-email")]
    [SwaggerOperation(Summary = "Confirm an account email with the mailed code", Tags = [UsersEndpoint.Tag])]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public override async Task<ActionResult> HandleAsync(
        ConfirmEmailCommand request,
        CancellationToken cancellationToken = default)
    {
        await dispatcher.SendAsync(request, cancellationToken);
        return Ok();
    }
}
