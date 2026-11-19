using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Platforms.Application.UserPlatforms.Queries.GetUserPlatformConnection;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Platforms.Api.Endpoints.UserPlatforms.Queries.GetUserPlatformConnection;

[Authorize]
internal sealed class GetUserPlatformConnectionEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<Guid>.WithActionResult<UserPlatformConnectionDto>
{
    [HttpGet(UserPlatformsEndpoint.BasePath + "/{userPlatformId:guid}/connection")]
    [SwaggerOperation(
        Summary = "Get user platform connection credentials",
        Description = "Returns OAuth token and external account id for publishing. Owner or admin only.",
        Tags = [UserPlatformsEndpoint.Tag])]
    [ProducesResponseType(typeof(UserPlatformConnectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<ActionResult<UserPlatformConnectionDto>> HandleAsync(
        [FromRoute] Guid userPlatformId,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(new GetUserPlatformConnectionQuery(userPlatformId), cancellationToken);
        return Ok(result);
    }
}
