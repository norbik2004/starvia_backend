using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Media.Application.MediaFiles.Commands.RemoveFile;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Media.Api.Endpoints.MediaFiles.Commands.RemoveFile;

[Authorize]
internal sealed class RemoveFileEndpoint(IDispatcher dispatcher, IContext context) : EndpointBaseAsync.WithRequest<RemoveFileRequest>.WithActionResult
{
    [HttpDelete(MediaFilesEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Delete file", Tags = [MediaFilesEndpoint.Tag])]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async override Task<ActionResult> HandleAsync([FromBody] RemoveFileRequest request, CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId || context.Identity.Email is not { } userEmail)
        {
            return Unauthorized();
        }

        var command = new RemoveFileCommand(request.FileId, userId, userEmail);
        await dispatcher.SendAsync(command, cancellationToken);

        return NoContent();
    }
}
