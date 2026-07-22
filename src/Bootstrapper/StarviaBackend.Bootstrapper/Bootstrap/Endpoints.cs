using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace StarviaBackend.Bootstrapper.Bootstrap;

internal static class Endpoints
{
    public static WebApplication MapAppEndpoints(this WebApplication app)
    {
        app.MapGet("/v1/health/live", () => Results.Ok(new { status = "live" }))
            .AllowAnonymous()
            .WithTags("Health");

        app.MapGet("/v1/health/ready", () => Results.Ok(new { status = "ready" }))
            .AllowAnonymous()
            .WithTags("Health");

        return app;
    }
}