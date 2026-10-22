using Microsoft.AspNetCore.Http;
using StarviaBackend.Shared.Abstractions.Contexts;

namespace StarviaBackend.Shared.Infrastructure.Contexts;

internal sealed class Context : IContext
{
    public string RequestId { get; }
    public string CorrelationId { get; }
    public string? TraceId { get; }
    public IIdentityContext Identity { get; }

    public Context(IHttpContextAccessor accessor)
    {
        RequestId = Guid.NewGuid().ToString("N");
        var httpContext = accessor.HttpContext;

        if (httpContext is null)
        {
            CorrelationId = RequestId;
            Identity = IdentityContext.Empty;
            return;
        }

        CorrelationId =
            httpContext.Request.Headers.TryGetValue(CorrelationIdMiddleware.HeaderName, out var header)
            && !string.IsNullOrWhiteSpace(header)
                ? header.ToString()
                : httpContext.TraceIdentifier;
        TraceId = httpContext.TraceIdentifier;
        Identity = new IdentityContext(httpContext.User);
    }
}
