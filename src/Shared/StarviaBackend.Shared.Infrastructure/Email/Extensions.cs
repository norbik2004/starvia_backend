using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Shared.Abstractions.Email;

namespace StarviaBackend.Shared.Infrastructure.Email;

internal static class Extensions
{
    public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(EmailSettings.SectionName);
        services.AddOptions<EmailSettings>().Bind(section);
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        return services;
    }
}
