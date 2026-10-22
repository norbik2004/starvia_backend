using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Shared.Infrastructure.Cqrs;

public static class Extensions
{
    /// <summary>Registers the command/query dispatchers and the <see cref="IDispatcher"/> facade.</summary>
    public static IServiceCollection AddDispatchers(this IServiceCollection services)
    {
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddScoped<IDispatcher, InMemoryDispatcher>();
        return services;
    }
}
