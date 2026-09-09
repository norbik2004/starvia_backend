using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Emails.Application.Emails.Templates;
using StarviaBackend.Modules.Emails.Core.Emails.Repositories;
using StarviaBackend.Modules.Emails.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Emails.Infrastructure.EF.Emails.Repositories;
using StarviaBackend.Modules.Emails.Infrastructure.EF.Initializers;
using StarviaBackend.Modules.Emails.Infrastructure.Mailing;
using StarviaBackend.Modules.Emails.Infrastructure.Mailing.Templates;
using StarviaBackend.Shared.Infrastructure.Cqrs;
using StarviaBackend.Shared.Infrastructure.Postgres;

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
        services.AddSingleton<IEmailTemplateRenderer, EmailTemplateRenderer>();
        services.AddSingleton<IRazorEmailRenderer, RazorEmailRenderer>();
        services.AddHostedService<EmailsDataInitializer>();

        services.RegisterHandlers(Assembly.GetExecutingAssembly());
        return services;
    }
}
