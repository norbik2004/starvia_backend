using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Brands.Application.BrandProfiles.Commands.CreateBrandProfile;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Brands.Api.Endpoints.BrandProfiles.Commands.CreateBrandProfile;

[Authorize]
internal sealed class CreateBrandProfileEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<CreateBrandProfileRequest>.WithActionResult<CreateBrandProfileResult>
{
    [HttpPost(BrandProfilesEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Create brand profile", Tags = [BrandProfilesEndpoint.Tag])]
    [ProducesResponseType(typeof(CreateBrandProfileResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult<CreateBrandProfileResult>> HandleAsync(
        [FromBody] CreateBrandProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await dispatcher.SendAsync<CreateBrandProfileCommand, CreateBrandProfileResult>(
            new CreateBrandProfileCommand(request, userId),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}
