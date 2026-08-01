using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Platforms.Application.UserPlatforms.Queries.BrowseUserPlatforms;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Platforms.Api.Endpoints.UserPlatforms.Queries.BrowseUserPlatforms;

[Authorize(Roles = UserPlatformsEndpoint.AdminRole)]
internal sealed class BrowseUserPlatformsEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<BrowseUserPlatformsQuery>.WithActionResult<PagedResult<UserPlatformDto>>
{
    [HttpGet(UserPlatformsEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Browse users (admin only)", Tags = [UserPlatformsEndpoint.Tag])]
    [ProducesResponseType(typeof(PagedResult<UserPlatformDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<PagedResult<UserPlatformDto>>> HandleAsync(
        [FromQuery] BrowseUserPlatformsQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(request, cancellationToken);
        return Ok(result);
    }
}
