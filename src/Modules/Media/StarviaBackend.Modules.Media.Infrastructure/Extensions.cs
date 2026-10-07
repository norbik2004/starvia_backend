using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Shared.Infrastructure.Cqrs;

[assembly:InternalsVisibleTo("StarviaBackend.Modules.Media.Api")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Media.Tests.Integration")]
namespace StarviaBackend.Modules.Media.Infrastructure;

internal static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {

        services.RegisterHandlers(Assembly.GetExecutingAssembly());
        return services;
    }
}
