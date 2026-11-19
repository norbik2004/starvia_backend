using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace StarviaBackend.Modules.Platforms.Integrations;

public static class Extensions
{
    /// <summary>
    /// Registers the platforms integration as a typed HttpClient behind <see cref="IPlatformsClient"/>.
    /// Call this from the consuming module's <c>AddInfrastructure</c>. Safe to call from multiple modules.
    /// </summary>
    public static IServiceCollection AddPlatformsIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        if (services.Any(descriptor => descriptor.ServiceType == typeof(PlatformsIntegrationMarker)))
        {
            return services;
        }

        services.AddSingleton<PlatformsIntegrationMarker>();
        services.AddOptions<PlatformsOptions>().Bind(configuration.GetSection(PlatformsOptions.SectionName));
        services.TryAddTransient<ForwardAuthorizationHandler>();

        services.AddHttpClient<IPlatformsClient, PlatformsClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<PlatformsOptions>>().Value;
            var baseUrl = options.BaseUrl.EndsWith('/') ? options.BaseUrl : $"{options.BaseUrl}/";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            if (!string.IsNullOrWhiteSpace(options.ApiKey))
            {
                client.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);
            }
        }).AddHttpMessageHandler<ForwardAuthorizationHandler>();

        return services;
    }

    private sealed class PlatformsIntegrationMarker;
}
