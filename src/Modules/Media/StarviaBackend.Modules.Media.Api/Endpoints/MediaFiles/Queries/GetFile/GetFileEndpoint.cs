using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Media.Application.MediaFiles.Queries.GetFile;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Media.Api.Endpoints.MediaFiles.Queries.GetFile;

[Authorize]
internal sealed class GetFileEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<Guid>.WithActionResult<FileLongDto>
{
    [HttpGet($"{MediaFilesEndpoint.BasePath}/{{fileId:guid}}")]
    [SwaggerOperation(Summary = "Get file by Id", Tags = [MediaFilesEndpoint.Tag])]
    [ProducesResponseType(typeof(FileLongDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<ActionResult<FileLongDto>> HandleAsync([FromRoute] Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFileQuery(fileId);
        var result = await dispatcher.QueryAsync(query, cancellationToken);

        return Ok(result);
    }
}
