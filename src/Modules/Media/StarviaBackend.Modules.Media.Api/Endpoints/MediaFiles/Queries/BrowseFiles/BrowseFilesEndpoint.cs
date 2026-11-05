using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Media.Application.MediaFiles.Queries.BrowseFiles;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Media.Api.Endpoints.MediaFiles.Queries.BrowseFiles;

[Authorize]
internal sealed class BrowseFilesEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<BrowseFilesQuery>.WithActionResult<PagedResult<FileDto>>
{
    [HttpGet(MediaFilesEndpoint.BasePath)]
    [SwaggerOperation(
        Summary = "Browse files",
        Description = "Admins see all files, other users only their own. Supports filtering by type, source, name/description text and uploader, and sorting.",
        Tags = [MediaFilesEndpoint.Tag])]
    [ProducesResponseType(typeof(PagedResult<FileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult<PagedResult<FileDto>>> HandleAsync([FromQuery] BrowseFilesQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(request, cancellationToken);

        return Ok(result);
    }
}
