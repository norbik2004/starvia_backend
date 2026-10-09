using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Shared.Infrastructure.Cqrs;

[assembly:InternalsVisibleTo("StarviaBackend.Modules.Media.Infrastructure")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Media.Api")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Media.Tests.Integration")]
namespace StarviaBackend.Modules.MediaFiles.Application;

internal static class Extensions
{
    /// <summary>Registers this assembly's command handlers and validators.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.RegisterHandlers(Assembly.GetExecutingAssembly());
        return services;
    }
}
