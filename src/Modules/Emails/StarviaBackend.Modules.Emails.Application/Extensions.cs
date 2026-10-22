using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Emails.Application.Emails.Messages;
using StarviaBackend.Shared.Infrastructure.Cqrs;

[assembly:InternalsVisibleTo("StarviaBackend.Modules.Emails.Infrastructure")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Emails.Api")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Emails.Tests.Integration")]
namespace StarviaBackend.Modules.Emails.Application;

internal static class Extensions
{
    /// <summary>Registers this assembly's command handlers and validators.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ConfirmAccountEmail>();
        services.AddScoped<ResetPasswordEmail>();
        services.AddScoped<WelcomingEmail>();
        services.RegisterHandlers(Assembly.GetExecutingAssembly());
        return services;
    }
}
