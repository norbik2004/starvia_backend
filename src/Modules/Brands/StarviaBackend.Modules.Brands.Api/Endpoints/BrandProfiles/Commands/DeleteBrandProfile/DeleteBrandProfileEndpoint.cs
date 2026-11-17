using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Brands.Application.BrandProfiles.Commands.DeleteBrandProfile;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Brands.Api.Endpoints.BrandProfiles.Commands.DeleteBrandProfile;

[Authorize]
internal sealed class DeleteBrandProfileEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<Guid>.WithActionResult
{
    [HttpDelete($"{BrandProfilesEndpoint.BasePath}/{{brandProfileId:guid}}")]
    [SwaggerOperation(Summary = "Delete brand profile", Tags = [BrandProfilesEndpoint.Tag])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public override async Task<ActionResult> HandleAsync(
        [FromRoute] Guid brandProfileId,
        CancellationToken cancellationToken = default)
    {
        await dispatcher.SendAsync(new DeleteBrandProfileCommand(brandProfileId), cancellationToken);
        return NoContent();
    }
}
