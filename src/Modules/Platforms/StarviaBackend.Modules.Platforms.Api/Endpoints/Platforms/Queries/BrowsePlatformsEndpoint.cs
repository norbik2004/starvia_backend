using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Platforms.Application.Platforms.Queries.BrowsePlatforms;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Platforms.Api.Endpoints.Platforms.Queries;

internal sealed class BrowsePlatformsEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<BrowsePlatformsQuery>.WithActionResult<PagedResult<PlatformDto>>
{
    [HttpGet(PlatformsEndpoint.BasePath)]
    [SwaggerOperation(
        Summary = "Browse the platforms catalogue",
        Description = "Public reference data; supports filtering by platform type and online status.",
        Tags = [PlatformsEndpoint.Tag])]
    [ProducesResponseType(typeof(PagedResult<PlatformDto>), StatusCodes.Status200OK)]
    public override async Task<ActionResult<PagedResult<PlatformDto>>> HandleAsync(
        [FromQuery] BrowsePlatformsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return Ok(result);
    }
}
