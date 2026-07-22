using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Shared.Infrastructure.Exceptions;

internal sealed class ErrorHandlerMiddleware(
    RequestDelegate next,
    IExceptionToResponseMapper mapper,
    ILogger<ErrorHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
            var response = mapper.Map(exception);
            context.Response.StatusCode = response.StatusCode;
            await context.Response.WriteAsJsonAsync(response.Response);
        }
    }
}
