using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Brands.Application.BrandProfiles;
using StarviaBackend.Modules.Brands.Application.BrandProfiles.Queries.GetBrandProfile;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Brands.Api.Endpoints.BrandProfiles.Queries.GetBrandProfile;

[Authorize]
internal sealed class GetBrandProfileEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<Guid>.WithActionResult<BrandProfileDto>
{
    [HttpGet($"{BrandProfilesEndpoint.BasePath}/{{brandProfileId:guid}}")]
    [SwaggerOperation(Summary = "Get brand profile by id", Tags = [BrandProfilesEndpoint.Tag])]
    [ProducesResponseType(typeof(BrandProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public override async Task<ActionResult<BrandProfileDto>> HandleAsync(
        [FromRoute] Guid brandProfileId,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(new GetBrandProfileQuery(brandProfileId), cancellationToken);
        return Ok(result);
    }
}
