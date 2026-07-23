using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Accounts.Api.Endpoints.Users;
using StarviaBackend.Modules.Accounts.Application.Users.Queries.BrowseUsers;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Accounts.Api.Endpoints.Users.Queries.BrowseUsers;

[Authorize(Roles = UsersEndpoint.AdminRole)]
internal sealed class BrowseUsersEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<BrowseUsersQuery>.WithActionResult<PagedResult<UserDto>>
{
    [HttpGet(UsersEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Browse users (admin only)", Tags = [UsersEndpoint.Tag])]
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<PagedResult<UserDto>>> HandleAsync(
        [FromQuery] BrowseUsersQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(request, cancellationToken);
        return Ok(result);
    }
}
