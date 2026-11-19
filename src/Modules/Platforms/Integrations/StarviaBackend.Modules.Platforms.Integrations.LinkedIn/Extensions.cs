using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace StarviaBackend.Modules.Platforms.Integrations.LinkedIn;

public static class Extensions
{
    /// <summary>
    /// Registers the LinkedIn integration as a typed HttpClient behind <see cref="ILinkedInClient"/>.
    /// Safe to call from multiple modules: only the first registration wins.
    /// </summary>
    public static IServiceCollection AddLinkedInIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        if (services.Any(descriptor => descriptor.ServiceType == typeof(LinkedInIntegrationMarker)))
        {
            return services;
        }

        services.AddSingleton<LinkedInIntegrationMarker>();
        services.AddOptions<LinkedInOptions>().Bind(configuration.GetSection(LinkedInOptions.SectionName));

        services.AddHttpClient<ILinkedInClient, LinkedInClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<LinkedInOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds <= 0 ? 30 : options.TimeoutSeconds);
        });

        return services;
    }

    private sealed class LinkedInIntegrationMarker;
}
