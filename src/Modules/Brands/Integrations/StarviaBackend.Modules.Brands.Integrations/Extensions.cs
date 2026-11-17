using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace StarviaBackend.Modules.Brands.Integrations;

public static class Extensions
{
    /// <summary>
    /// Registers the brands integration as a typed HttpClient behind <see cref="IBrandsClient"/>.
    /// Call this from the consuming module's <c>AddInfrastructure</c>. Safe to call from multiple
    /// modules: only the first registration wins.
    /// </summary>
    public static IServiceCollection AddBrandsIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        if (services.Any(descriptor => descriptor.ServiceType == typeof(BrandsIntegrationMarker)))
        {
            return services;
        }

        services.AddSingleton<BrandsIntegrationMarker>();
        services.AddOptions<BrandsOptions>().Bind(configuration.GetSection(BrandsOptions.SectionName));
        services.TryAddTransient<ForwardAuthorizationHandler>();

        services.AddHttpClient<IBrandsClient, BrandsClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<BrandsOptions>>().Value;
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

    private sealed class BrandsIntegrationMarker;
}
