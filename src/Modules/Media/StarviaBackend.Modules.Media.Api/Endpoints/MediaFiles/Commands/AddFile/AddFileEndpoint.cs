using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Media.Application.MediaFiles.Commands.AddFile;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Media.Api.Endpoints.MediaFiles.Commands.AddFile;

/// <summary>Multipart form: the file itself plus optional metadata.</summary>
public sealed class AddFileFormRequest
{
    public IFormFile File { get; set; } = default!;
    public string? Description { get; set; }
}

[Authorize]
internal sealed class AddFileEndpoint(IDispatcher dispatcher, IContext context)
    : EndpointBaseAsync.WithRequest<AddFileFormRequest>.WithActionResult<AddFileResult>
{
    private const long MaxRequestBodySize = 55 * 1024 * 1024;

    [HttpPost(MediaFilesEndpoint.BasePath)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBodySize)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBodySize)]
    [SwaggerOperation(Summary = "Add file", Tags = [MediaFilesEndpoint.Tag])]
    [ProducesResponseType(typeof(AddFileResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public override async Task<ActionResult<AddFileResult>> HandleAsync([FromForm] AddFileFormRequest request,
        CancellationToken cancellationToken = default)
    {
        if (context.Identity.UserId is not { } userId || context.Identity.Email is not { } userEmail)
        {
            return Unauthorized();
        }

        if (request.File is null)
        {
            return BadRequest("File is required.");
        }

        await using var content = request.File.OpenReadStream();

        var fileRequest = new AddFileRequest(
            request.File.FileName,
            request.File.ContentType,
            request.File.Length,
            content,
            request.Description);

        var command = new AddFileCommand(fileRequest, userId, userEmail);
        var result = await dispatcher.SendAsync<AddFileCommand, AddFileResult>(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}
