using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Accounts.Api.Endpoints.Users;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.ChangeUserName;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.RegisterUser;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Exceptions;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Accounts.Api.Endpoints.Users.Commands.ChangeUserName;

[AllowAnonymous]
internal sealed class ChangeUserNameEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<ChangeUserNameRequest>.WithActionResult<ChangeUserNameResult>
{
    [HttpPut($"{UsersEndpoint.BasePath}/change-user-name")]
    [SwaggerOperation(Summary = "Change account's username", Tags = [UsersEndpoint.Tag])]
    [ProducesResponseType(typeof(RegisterUserResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public override async Task<ActionResult<ChangeUserNameResult>> HandleAsync(
        ChangeUserNameRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new ChangeUserNameCommand(request.UserName, userId);

        var result = await dispatcher.SendAsync<ChangeUserNameCommand, ChangeUserNameResult>(command, cancellationToken);
        return Ok(result);
    }
}
