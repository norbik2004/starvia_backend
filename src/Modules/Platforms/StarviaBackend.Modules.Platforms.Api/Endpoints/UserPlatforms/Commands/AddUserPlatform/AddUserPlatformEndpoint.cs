using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Accounts.Api.Endpoints.Users;
using StarviaBackend.Modules.Platforms.Application.UserPlatforms.Commands.AddUserPlatform;
using StarviaBackend.Modules.Platforms.Application.UserPlatforms.Queries.BrowseUserPlatforms;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Platforms.Api.Endpoints.UserPlatforms.Commands.AddUserPlatform;

[Authorize]
internal sealed class AddUserPlatformEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<AddUserPlatformRequest>.WithActionResult<AddUserPlatformResult>
{
    [HttpPost(UserPlatformsEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Add user platform", Tags = [UserPlatformsEndpoint.Tag])]
    [ProducesResponseType(typeof(PagedResult<AddUserPlatformResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<AddUserPlatformResult>> HandleAsync(
        [FromForm] AddUserPlatformRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new AddUserPlatformCommand(request, userId);

        var result = await dispatcher.SendAsync<AddUserPlatformCommand, AddUserPlatformResult>(command, cancellationToken);
        return Ok(result);
    }
}
