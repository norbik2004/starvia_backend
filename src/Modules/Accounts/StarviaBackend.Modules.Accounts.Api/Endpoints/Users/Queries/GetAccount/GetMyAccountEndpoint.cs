using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Accounts.Api.Endpoints.Users;
using StarviaBackend.Modules.Accounts.Application.Users.Queries.GetAccount;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Accounts.Api.Endpoints.Users.Queries.GetAccount;

[Authorize]
internal sealed class GetMyAccountEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithoutRequest.WithActionResult<AccountDto>
{
    [HttpGet($"{UsersEndpoint.BasePath}/me")]
    [SwaggerOperation(Summary = "Get the current account", Tags = [UsersEndpoint.Tag])]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult<AccountDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var account = await dispatcher.QueryAsync(new GetAccountQuery(userId), cancellationToken);
        return Ok(account);
    }
}
