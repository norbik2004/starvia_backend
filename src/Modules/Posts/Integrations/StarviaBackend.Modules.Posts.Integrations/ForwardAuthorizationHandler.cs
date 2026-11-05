using Microsoft.AspNetCore.Http;

namespace StarviaBackend.Modules.Posts.Integrations;

/// <summary>
/// Forwards the caller's bearer token so the posts API's authorization runs as that user.
/// </summary>
internal sealed class ForwardAuthorizationHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains("Authorization"))
        {
            var authorization = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(authorization))
            {
                throw new PostsIntegrationException(
                    "Cannot call the posts API without the caller's Authorization header. "
                    + "The posts integration must run inside an authenticated HTTP request.");
            }

            request.Headers.TryAddWithoutValidation("Authorization", authorization);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
