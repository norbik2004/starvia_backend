using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Shared.Abstractions.Contexts;

namespace StarviaBackend.Shared.Infrastructure.Contexts;

public static class Extensions
{
    public static IServiceCollection AddContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IContext, Context>();
        return services;
    }

    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}
