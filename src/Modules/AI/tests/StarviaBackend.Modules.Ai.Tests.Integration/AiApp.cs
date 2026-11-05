using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.AI;
using StarviaBackend.Modules.Ai.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Posts.Integrations;
using StarviaBackend.Shared.Tests.Integration;

namespace StarviaBackend.Modules.Ai.Tests.Integration;

/// <summary>Test host for the AI module: applies its migrations and swaps the text chat client (Google AI Studio) and posts integration for fakes.</summary>
public sealed class AiApp : BoilerplateApp
{
    /// <summary>The model that writes the texts in these tests, in place of Google AI Studio.</summary>
    public FakeChatClient TextModel { get; } = new();

    public FakePostsClient Posts { get; } = new();

    /// <summary>
    /// Lets one test run against the application's own Google AI Studio registration instead of the fake. It never
    /// reaches the internet: the key is blanked below, so the client refuses to be built. Reset by every test.
    /// </summary>
    public bool UseRealChatClient { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        // A key in the developer's environment must never turn a test into a billed or quota-consuming call.
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["integrations:googleaistudio:ApiKey"] = string.Empty,
                ["integrations:openrouter:ApiKey"] = string.Empty,
            }));

        builder.ConfigureTestServices(services =>
        {
            // The module's one plain chat client is Google AI Studio's; OpenRouter is registered by key and is
            // not touched here. Single() also fails the host if a second plain client ever shows up.
            var registered = services.Single(descriptor => descriptor.ServiceType == typeof(IChatClient) && !descriptor.IsKeyedService);

            services.Remove(registered);
            services.AddScoped<IChatClient>(provider => UseRealChatClient ? (IChatClient)registered.ImplementationFactory!(provider) : TextModel);

            services.RemoveAll<IPostsClient>();
            services.AddSingleton<IPostsClient>(Posts);
        });
    }

    protected override async Task RunMigrationsAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AiWriteDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
