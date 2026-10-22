using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Media.Application.MediaFiles.Queries.BrowseFiles;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Media.Api.Endpoints.MediaFiles.Queries.BrowseFiles;

[Authorize]
internal sealed class BrowseFilesEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<BrowseFilesRequest>.WithActionResult<PagedResult<FileDto>>
{
    [HttpGet(MediaFilesEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Browse my files", Tags = [MediaFilesEndpoint.Tag])]
    [ProducesResponseType(typeof(PagedResult<FileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult<PagedResult<FileDto>>> HandleAsync([FromQuery] BrowseFilesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var query = new BrowseFilesQuery(userId, request.Page, request.PageSize);
        var result = await dispatcher.QueryAsync(query, cancellationToken);

        return Ok(result);
    }
}
