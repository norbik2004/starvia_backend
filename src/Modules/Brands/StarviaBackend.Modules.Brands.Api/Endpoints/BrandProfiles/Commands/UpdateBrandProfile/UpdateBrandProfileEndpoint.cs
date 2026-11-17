using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Brands.Application.BrandProfiles.Commands.UpdateBrandProfile;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Models;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Platforms;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Brands.Api.Endpoints.BrandProfiles.Commands.UpdateBrandProfile;

public sealed record UpdateBrandProfileBody(
    string Name,
    BrandBrief Brief,
    IReadOnlyList<PlatformType> Platforms,
    BrandGenerationDefaults? GenerationDefaults);

[Authorize]
internal sealed class UpdateBrandProfileEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<UpdateBrandProfileEndpointRequest>.WithActionResult
{
    [HttpPut($"{BrandProfilesEndpoint.BasePath}/{{brandProfileId:guid}}")]
    [SwaggerOperation(Summary = "Update brand profile", Tags = [BrandProfilesEndpoint.Tag])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult> HandleAsync(
        UpdateBrandProfileEndpointRequest request,
        CancellationToken cancellationToken = default)
    {
        await dispatcher.SendAsync(
            new UpdateBrandProfileCommand(new UpdateBrandProfileRequest(
                request.BrandProfileId,
                request.Body.Name,
                request.Body.Brief,
                request.Body.Platforms,
                request.Body.GenerationDefaults)),
            cancellationToken);

        return NoContent();
    }
}

public sealed class UpdateBrandProfileEndpointRequest
{
    [FromRoute(Name = "brandProfileId")]
    public Guid BrandProfileId { get; set; }

    [FromBody]
    public UpdateBrandProfileBody Body { get; set; } = null!;
}
