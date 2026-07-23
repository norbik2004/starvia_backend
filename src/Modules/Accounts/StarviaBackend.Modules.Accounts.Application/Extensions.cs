using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Shared.Infrastructure.Cqrs;

namespace StarviaBackend.Modules.Accounts.Application;

internal static class Extensions
{
    /// <summary>Registers this assembly's command handlers and validators.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.RegisterHandlers(Assembly.GetExecutingAssembly());
        return services;
    }
}
