using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Media.Integrations;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Core.Posts.Repositories;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Initializers;
using StarviaBackend.Modules.Posts.Infrastructure.EF.PostAttachments.Repositories;
using StarviaBackend.Modules.Posts.Infrastructure.EF.Posts.Repositories;
using StarviaBackend.Modules.Posts.Infrastructure.EF.PostsPublications.Repositories;
using StarviaBackend.Shared.Infrastructure.Cqrs;
using StarviaBackend.Shared.Infrastructure.Postgres;

[assembly:InternalsVisibleTo("StarviaBackend.Modules.Posts.Api")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Posts.Tests.Integration")]
namespace StarviaBackend.Modules.Posts.Infrastructure;

internal static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgres<PostWriteDbContext>();
        services.AddPostgres<PostReadDbContext>();

        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IPostPublicationRepository, PostPublicationRepository>();
        services.AddScoped<IPostAttachmentRepository, PostAttachmentRepository>();

        services.AddMediaIntegration(configuration);

        services.AddHostedService<PostDataInitializer>();

        services.RegisterHandlers(Assembly.GetExecutingAssembly());
        return services;
    }
}
