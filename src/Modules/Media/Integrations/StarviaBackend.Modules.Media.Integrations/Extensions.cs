using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace StarviaBackend.Modules.Media.Integrations;

public static class Extensions
{
    public static IServiceCollection AddMediaIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        if (services.Any(descriptor => descriptor.ServiceType == typeof(MediaIntegrationMarker)))
        {
            return services;
        }

        services.AddSingleton<MediaIntegrationMarker>();
        services.AddOptions<MediaOptions>().Bind(configuration.GetSection(MediaOptions.SectionName));
        services.TryAddTransient<ForwardAuthorizationHandler>();

        services.AddHttpClient<IMediaClient, MediaClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<MediaOptions>>().Value;
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

    private sealed class MediaIntegrationMarker;
}
