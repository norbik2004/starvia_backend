using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Posts.Application.Posts.Access;
using StarviaBackend.Shared.Infrastructure.Cqrs;

[assembly:InternalsVisibleTo("StarviaBackend.Modules.Posts.Infrastructure")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Posts.Api")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Posts.Tests.Integration")]
namespace StarviaBackend.Modules.Posts.Application;

internal static class Extensions
{
    /// <summary>Registers this assembly's command handlers and validators.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPostAccessService, PostAccessService>();
        services.RegisterHandlers(Assembly.GetExecutingAssembly());
        return services;
    }
}
