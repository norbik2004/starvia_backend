using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Calendar.Core.Calendars.Repositories;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.CalendarEntries.Repositories;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.ContentCalendars.Repositories;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Calendar.Infrastructure.EF.Initializers;
using StarviaBackend.Shared.Infrastructure.Cqrs;
using StarviaBackend.Shared.Infrastructure.Postgres;

[assembly:InternalsVisibleTo("StarviaBackend.Modules.Calendar.Api")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Calendar.Tests.Integration")]
namespace StarviaBackend.Modules.Calendar.Infrastructure;

internal static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgres<CalendarWriteDbContext>();
        services.AddPostgres<CalendarReadDbContext>();

        services.AddScoped<ICalendarEntryRepository, CalendarEntryRepository>();
        services.AddScoped<ICalendarRepository, ContentCalendarRepository>();

        services.AddHostedService<CalendarDataInitializer>();

        services.RegisterHandlers(Assembly.GetExecutingAssembly());
        return services;
    }
}
