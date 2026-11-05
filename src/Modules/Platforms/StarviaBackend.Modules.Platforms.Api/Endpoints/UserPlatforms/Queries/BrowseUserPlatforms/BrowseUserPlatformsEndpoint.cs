using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Platforms.Application.UserPlatforms.Queries.BrowseUserPlatforms;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Platforms.Api.Endpoints.UserPlatforms.Queries.BrowseUserPlatforms;

[Authorize]
internal sealed class BrowseUserPlatformsEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<BrowseUserPlatformsQuery>.WithActionResult<PagedResult<UserPlatformDto>>
{
    [HttpGet(UserPlatformsEndpoint.BasePath)]
    [SwaggerOperation(
        Summary = "Browse connected platform accounts",
        Description = "Admins see all accounts, other users only their own. Supports filtering by platform, account name and owner, and sorting.",
        Tags = [UserPlatformsEndpoint.Tag])]
    [ProducesResponseType(typeof(PagedResult<UserPlatformDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult<PagedResult<UserPlatformDto>>> HandleAsync(
        [FromQuery] BrowseUserPlatformsQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(request, cancellationToken);
        return Ok(result);
    }
}
