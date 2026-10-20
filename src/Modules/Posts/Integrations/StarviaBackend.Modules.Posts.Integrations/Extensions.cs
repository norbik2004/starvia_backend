using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace StarviaBackend.Modules.Posts.Integrations;

public static class Extensions
{
    /// <summary>
    /// Registers the posts integration as a typed HttpClient behind <see cref="IPostsClient"/>.
    /// Call this from the consuming module's <c>AddInfrastructure</c>.
    /// </summary>
    public static IServiceCollection AddPostsIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PostsOptions>().Bind(configuration.GetSection(PostsOptions.SectionName));
        services.AddTransient<ForwardAuthorizationHandler>();

        services.AddHttpClient<IPostsClient, PostsClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<PostsOptions>>().Value;
            var baseUrl = options.BaseUrl.EndsWith('/') ? options.BaseUrl : $"{options.BaseUrl}/";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);
        }).AddHttpMessageHandler<ForwardAuthorizationHandler>();

        return services;
    }
}
