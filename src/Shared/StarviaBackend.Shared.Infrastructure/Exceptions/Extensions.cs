using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Shared.Infrastructure.Exceptions;

public static class Extensions
{
    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        services.AddSingleton<IExceptionToResponseMapper, ExceptionToResponseMapper>();
        return services;
    }

    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ErrorHandlerMiddleware>();
}
