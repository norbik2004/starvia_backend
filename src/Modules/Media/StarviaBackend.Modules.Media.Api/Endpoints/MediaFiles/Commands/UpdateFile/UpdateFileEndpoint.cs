using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Media.Application.MediaFiles.Commands.UpdateFile;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Media.Api.Endpoints.MediaFiles.Commands.UpdateFile;

[Authorize]
internal sealed class UpdateFileEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<UpdateFileRequest>.WithActionResult<UpdateFileResult>
{
    [HttpPut(MediaFilesEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Update file", Tags = [MediaFilesEndpoint.Tag])]
    [ProducesResponseType(typeof(UpdateFileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<UpdateFileResult>> HandleAsync([FromBody] UpdateFileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var command = new UpdateFileCommand(request, userId);
        var result = await dispatcher.SendAsync<UpdateFileCommand, UpdateFileResult>(command, cancellationToken);

        return Ok(result);
    }
}
