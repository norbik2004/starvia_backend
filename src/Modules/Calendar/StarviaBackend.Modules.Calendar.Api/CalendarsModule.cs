using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Calendar.Core;
using StarviaBackend.Modules.Calendar.Infrastructure;
using StarviaBackend.Modules.Calendar.Application;

namespace StarviaBackend.Modules.Calendar.Api;

public static class CalendarsModule
{
    public static IServiceCollection RegisterCalendarsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCore();
        services.AddApplication();
        services.AddInfrastructure(configuration);
        return services;
    }
}
