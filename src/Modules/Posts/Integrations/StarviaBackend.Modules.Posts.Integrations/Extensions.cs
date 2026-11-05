using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace StarviaBackend.Modules.Posts.Integrations;

public static class Extensions
{
    /// <summary>
    /// Registers the posts integration as a typed HttpClient behind <see cref="IPostsClient"/>.
    /// Call this from the consuming module's <c>AddInfrastructure</c>. Safe to call from multiple
    /// modules: only the first registration wins (duplicate <c>AddHttpClient</c> for the same
    /// typed client breaks the Authorization-forwarding handler and surfaces as HTTP 401).
    /// </summary>
    public static IServiceCollection AddPostsIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        if (services.Any(descriptor => descriptor.ServiceType == typeof(PostsIntegrationMarker)))
        {
            return services;
        }

        services.AddSingleton<PostsIntegrationMarker>();
        services.AddOptions<PostsOptions>().Bind(configuration.GetSection(PostsOptions.SectionName));
        services.TryAddTransient<ForwardAuthorizationHandler>();

        services.AddHttpClient<IPostsClient, PostsClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<PostsOptions>>().Value;
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

    private sealed class PostsIntegrationMarker;
}
