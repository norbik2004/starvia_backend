using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Brands.Application.BrandProfiles;
using StarviaBackend.Modules.Brands.Application.BrandProfiles.Queries.BrowseBrandProfilesByUserPlatform;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Brands.Api.Endpoints.BrandProfiles.Queries.BrowseBrandProfilesByUserPlatform;

[Authorize]
internal sealed class BrowseBrandProfilesByUserPlatformEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<Guid>.WithActionResult<IReadOnlyList<BrandProfileSummaryDto>>
{
    [HttpGet($"{BrandProfilesEndpoint.BasePath}/by-user-platform/{{userPlatformId:guid}}")]
    [SwaggerOperation(Summary = "List brand profiles linked to a user platform", Tags = [BrandProfilesEndpoint.Tag])]
    [ProducesResponseType(typeof(IReadOnlyList<BrandProfileSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult<IReadOnlyList<BrandProfileSummaryDto>>> HandleAsync(
        [FromRoute] Guid userPlatformId,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(
            new BrowseBrandProfilesByUserPlatformQuery(userPlatformId),
            cancellationToken);
        return Ok(result);
    }
}
