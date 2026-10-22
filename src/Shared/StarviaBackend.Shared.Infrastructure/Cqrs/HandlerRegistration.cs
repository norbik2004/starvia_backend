using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Shared.Infrastructure.Cqrs;

public static class HandlerRegistration
{
    /// <summary>
    /// Scans <paramref name="assembly"/> for command/query handlers and FluentValidation
    /// validators (including internal ones) and registers them scoped. Each module calls this
    /// for its own Application / Infrastructure assembly.
    /// </summary>
    public static IServiceCollection RegisterHandlers(this IServiceCollection services, Assembly assembly)
    {
        services.Scan(s => s.FromAssemblies(assembly)
            .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
            .AsImplementedInterfaces().WithScopedLifetime()

            .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
            .AsImplementedInterfaces().WithScopedLifetime()

            .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
            .AsImplementedInterfaces().WithScopedLifetime()

            .AddClasses(c => c.AssignableTo(typeof(IValidator<>)), publicOnly: false)
            .AsImplementedInterfaces().WithScopedLifetime());

        return services;
    }
}
