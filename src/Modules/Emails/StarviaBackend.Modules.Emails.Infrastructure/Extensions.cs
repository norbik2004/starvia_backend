using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Emails.Core.Emails.Entities;
using StarviaBackend.Modules.Emails.Core.Emails.Repositories;
using StarviaBackend.Shared.Infrastructure.Cqrs;
using StarviaBackend.Shared.Infrastructure.Postgres;
using StarviaBackend.Modules.Emails.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Emails.Infrastructure.EF.Emails.Repositories;

[assembly:InternalsVisibleTo("StarviaBackend.Modules.Emails.Api")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Emails.Tests.Integration")]
namespace StarviaBackend.Modules.Emails.Infrastructure;

internal static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddPostgres<EmailsWriteDbContext>();
        services.AddPostgres<EmailsReadDbContex>();

        services.AddScoped<IEmailRepository, EmailRepository>();

        services.RegisterHandlers(Assembly.GetExecutingAssembly());
        return services;
    }
}
