using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Brands.Application.BrandProfiles;
using StarviaBackend.Modules.Brands.Application.BrandProfiles.Queries.BrowseBrandProfiles;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Brands.Api.Endpoints.BrandProfiles.Queries.BrowseBrandProfiles;

[Authorize]
internal sealed class BrowseBrandProfilesEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<BrowseBrandProfilesQuery>.WithActionResult<PagedResult<BrandProfileSummaryDto>>
{
    [HttpGet(BrandProfilesEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Browse brand profiles", Tags = [BrandProfilesEndpoint.Tag])]
    [ProducesResponseType(typeof(PagedResult<BrandProfileSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult<PagedResult<BrandProfileSummaryDto>>> HandleAsync(
        [FromQuery] BrowseBrandProfilesQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return Ok(result);
    }
}
