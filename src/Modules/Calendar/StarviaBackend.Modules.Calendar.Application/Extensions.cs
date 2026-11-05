using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Calendar.Application.Access;
using StarviaBackend.Shared.Infrastructure.Cqrs;

[assembly:InternalsVisibleTo("StarviaBackend.Modules.Calendar.Infrastructure")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Calendar.Api")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Calendar.Tests.Integration")]
namespace StarviaBackend.Modules.Calendar.Application;

internal static class Extensions
{
    /// <summary>Registers this assembly's command handlers and validators.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICalendarAccessService, CalendarAccessService>();
        services.RegisterHandlers(Assembly.GetExecutingAssembly());
        return services;
    }
}
