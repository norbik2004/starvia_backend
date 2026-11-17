using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Brands.Application.BrandProfiles;
using StarviaBackend.Modules.Brands.Application.BrandProfiles.Commands.SetBrandProfileUserPlatforms;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Brands.Api.Endpoints.BrandProfiles.Commands.SetBrandProfileUserPlatforms;

public sealed record SetBrandProfileUserPlatformsBody(IReadOnlyList<BrandProfileUserPlatformLink> UserPlatforms);

[Authorize]
internal sealed class SetBrandProfileUserPlatformsEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<SetBrandProfileUserPlatformsEndpointRequest>.WithActionResult
{
    [HttpPut($"{BrandProfilesEndpoint.BasePath}/{{brandProfileId:guid}}/user-platforms")]
    [SwaggerOperation(Summary = "Set linked user platforms for a brand profile", Tags = [BrandProfilesEndpoint.Tag])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult> HandleAsync(
        SetBrandProfileUserPlatformsEndpointRequest request,
        CancellationToken cancellationToken = default)
    {
        await dispatcher.SendAsync(
            new SetBrandProfileUserPlatformsCommand(new SetBrandProfileUserPlatformsRequest(
                request.BrandProfileId,
                request.Body.UserPlatforms)),
            cancellationToken);

        return NoContent();
    }
}

public sealed class SetBrandProfileUserPlatformsEndpointRequest
{
    [FromRoute(Name = "brandProfileId")]
    public Guid BrandProfileId { get; set; }

    [FromBody]
    public SetBrandProfileUserPlatformsBody Body { get; set; } = null!;
}
