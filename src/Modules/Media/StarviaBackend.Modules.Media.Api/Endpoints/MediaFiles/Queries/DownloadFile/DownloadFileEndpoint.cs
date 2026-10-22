using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using StarviaBackend.Modules.Media.Application.MediaFiles;
using StarviaBackend.Modules.Media.Application.MediaFiles.Queries.DownloadFile;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Media.Api.Endpoints.MediaFiles.Queries.DownloadFile;

[Authorize]
internal sealed class DownloadFileEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<Guid>.WithActionResult
{
    private const string DefaultContentType = "application/octet-stream";

    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    [HttpGet($"{MediaFilesEndpoint.BasePath}/{{fileId:guid}}/{MediaFileRoutes.ContentSegment}")]
    [SwaggerOperation(Summary = "Download file content", Tags = [MediaFilesEndpoint.Tag])]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<ActionResult> HandleAsync([FromRoute] Guid fileId,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var query = new DownloadFileQuery(userId, fileId);
        var result = await dispatcher.QueryAsync(query, cancellationToken);

        if (!ContentTypeProvider.TryGetContentType(result.FileName, out var contentType))
        {
            contentType = DefaultContentType;
        }

        return File(result.Content, contentType);
    }
}
