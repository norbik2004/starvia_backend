using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Emails.Api.Endpoints.Emails;
using StarviaBackend.Modules.Emails.Application.Emails.Queries.BrowseEmails;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Emails.Api.Endpoints.Emails.Queries.BrowseEmails;

[Authorize(Roles = EmailsEndpoint.AdminRole)]
internal sealed class BrowseEmailsEndpoint(IDispatcher dispatcher)
    : EndpointBaseAsync.WithRequest<BrowseEmailsQuery>.WithActionResult<PagedResult<EmailDto>>
{
    [HttpGet(EmailsEndpoint.BasePath)]
    [SwaggerOperation(Summary = "Browse sent emails (admin only)", Tags = [EmailsEndpoint.Tag])]
    [ProducesResponseType(typeof(PagedResult<EmailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public override async Task<ActionResult<PagedResult<EmailDto>>> HandleAsync(
        [FromQuery] BrowseEmailsQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.QueryAsync(request, cancellationToken);
        return Ok(result);
    }
}
