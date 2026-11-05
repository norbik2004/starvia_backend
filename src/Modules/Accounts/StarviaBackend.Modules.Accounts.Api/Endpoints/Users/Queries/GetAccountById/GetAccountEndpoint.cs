using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Accounts.Application.Users.Queries.GetAccount;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Exceptions;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Accounts.Api.Endpoints.Users.Queries.GetAccountById;

[Authorize]
internal sealed class GetAccountEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<Guid>.WithActionResult<AccountDto>
{
    [HttpGet($"{UsersEndpoint.BasePath}/{{userId:guid}}")]
    [SwaggerOperation(Summary = "Get an account (its owner or an admin)", Tags = [UsersEndpoint.Tag])]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<AccountDto>> HandleAsync(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken = default)
    {
        var account = await dispatcher.QueryAsync(new GetAccountQuery(userId), cancellationToken);
        return Ok(account);
    }
}