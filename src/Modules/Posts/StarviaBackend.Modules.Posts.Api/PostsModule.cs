using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Posts.Core;
using StarviaBackend.Modules.Posts.Infrastructure;
using StarviaBackend.Modules.Posts.Application;

namespace StarviaBackend.Modules.Posts.Api;

public static class PostsModule
{
    public static IServiceCollection RegisterPostsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCore();
        services.AddApplication();
        services.AddInfrastructure(configuration);
        return services;
    }
}
