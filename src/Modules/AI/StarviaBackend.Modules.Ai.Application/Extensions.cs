using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Ai.Application.Chats.Access;
using StarviaBackend.Shared.Infrastructure.Cqrs;

[assembly:InternalsVisibleTo("StarviaBackend.Modules.Ai.Infrastructure")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Ai.Api")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Ai.Tests.Integration")]
namespace StarviaBackend.Modules.Ai.Application;

internal static class Extensions
{
    /// <summary>Registers this assembly's command handlers and validators.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.RegisterHandlers(Assembly.GetExecutingAssembly());
        services.AddScoped<IChatAccessService, ChatAccessService>();
        return services;
    }
}
