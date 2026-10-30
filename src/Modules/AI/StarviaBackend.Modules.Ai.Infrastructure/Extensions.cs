using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Ai.Core.Chats.Repositories;
using StarviaBackend.Modules.Ai.Infrastructure.EF.Chats.Repositories;
using StarviaBackend.Modules.Ai.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Ai.Infrastructure.EF.Initializers;
using StarviaBackend.Modules.Ai.Infrastructure.GoogleAiStudio;
using StarviaBackend.Modules.Ai.Infrastructure.OpenRouter;
using StarviaBackend.Shared.Infrastructure.Cqrs;
using StarviaBackend.Shared.Infrastructure.Postgres;

[assembly:InternalsVisibleTo("StarviaBackend.Modules.Ai.Api")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Ai.Tests.Integration")]
namespace StarviaBackend.Modules.Ai.Infrastructure;

internal static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgres<AiWriteDbContext>();
        services.AddPostgres<AiReadDbContext>();

        services.AddScoped<IChatRepository, ChatRepository>();

        // Every text is written by Google AI Studio. OpenRouter is registered by key and kept for the images.
        services.AddGoogleAiStudio(configuration);
        services.AddOpenRouter(configuration);

        services.AddHostedService<AiDataInitializer>();

        services.RegisterHandlers(Assembly.GetExecutingAssembly());
        return services;
    }
}
