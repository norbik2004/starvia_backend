using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Platforms.Application.UserPlatforms.Queries.GetUserPlatform;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Platforms.Api.Endpoints.UserPlatforms.Queries.GetUserPlatform;

[Authorize]
internal sealed class GetUserPlatformEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<Guid>.WithActionResult<UserPlatformDtoLong>
{
    [HttpGet($"{UserPlatformsEndpoint.BasePath}/{{userPlatformId:guid}}")]
    [SwaggerOperation(
        Summary = "Get a connected platform account by Id",
        Description = "Visible to the user who connected it and to admins.",
        Tags = [UserPlatformsEndpoint.Tag])]
    [ProducesResponseType(typeof(UserPlatformDtoLong), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<UserPlatformDtoLong>> HandleAsync(
        [FromRoute] Guid userPlatformId,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(new GetUserPlatformQuery(userPlatformId), cancellationToken);
        return Ok(result);
    }
}
