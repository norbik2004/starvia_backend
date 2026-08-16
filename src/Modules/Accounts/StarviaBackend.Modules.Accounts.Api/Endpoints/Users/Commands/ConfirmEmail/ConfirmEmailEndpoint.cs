using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Accounts.Api.Endpoints.Users;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.ConfirmEmail;
using StarviaBackend.Shared.Abstractions.App;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Exceptions;
using Swashbuckle.AspNetCore.Annotations;

namespace StarviaBackend.Modules.Accounts.Api.Endpoints.Users.Commands.ConfirmEmail;

[AllowAnonymous]
internal sealed class ConfirmEmailEndpoint(IDispatcher dispatcher, IAppUrls appUrls)
    : EndpointBaseAsync.WithRequest<ConfirmEmailRequest>.WithActionResult
{
    [HttpGet($"{UsersEndpoint.BasePath}/confirm-email")]
    [SwaggerOperation(
        Summary = "Confirm an account email from the mailed link, then redirect to the frontend",
        Tags = [UsersEndpoint.Tag])]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public override async Task<ActionResult> HandleAsync(
        [FromQuery] ConfirmEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await dispatcher.SendAsync(
                new ConfirmEmailCommand(request.UserId, request.Code ?? string.Empty),
                cancellationToken);

            return Redirect(EmailConfirmationLink.FrontendRedirect(appUrls.FrontendBaseUrl));
        }
        catch (BusinessException ex)
        {
            return Redirect(EmailConfirmationLink.FrontendRedirect(appUrls.FrontendBaseUrl, ex.Code));
        }
        catch (Exception)
        {
            return Redirect(EmailConfirmationLink.FrontendRedirect(appUrls.FrontendBaseUrl, "confirmation_failed"));
        }
    }
}

internal sealed class ConfirmEmailRequest
{
    public Guid UserId { get; set; }
    public string? Code { get; set; }
}


