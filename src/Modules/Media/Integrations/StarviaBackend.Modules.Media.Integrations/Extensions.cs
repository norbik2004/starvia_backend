using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace StarviaBackend.Modules.Media.Integrations;

public static class Extensions
{
    /// <summary>
    /// Registers the media integration as a typed HttpClient behind <see cref="IMediaClient"/>.
    /// Call this from the consuming module's <c>AddInfrastructure</c>.
    /// </summary>
    public static IServiceCollection AddMediaIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MediaOptions>().Bind(configuration.GetSection(MediaOptions.SectionName));
        services.AddTransient<ForwardAuthorizationHandler>();

        services.AddHttpClient<IMediaClient, MediaClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<MediaOptions>>().Value;
            var baseUrl = options.BaseUrl.EndsWith('/') ? options.BaseUrl : $"{options.BaseUrl}/";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);
        }).AddHttpMessageHandler<ForwardAuthorizationHandler>();

        return services;
    }
}
