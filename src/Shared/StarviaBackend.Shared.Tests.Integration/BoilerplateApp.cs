using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularMonolith.Bootstrapper;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace StarviaBackend.Shared.Tests.Integration;

/// <summary>
/// Base test host: boots the real Bootstrapper against a throwaway Testcontainers Postgres,
/// swaps JWT for <see cref="FakeAuthHandler"/>, and strips hosted services so nothing races the
/// fresh schema. Each module derives this and overrides <see cref="RunMigrationsAsync"/> to apply
/// its own migrations (and start whatever it needs, e.g. the bus).
/// </summary>
public class BoilerplateApp : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // "__" maps to ":" in config → binds PostgresOptions.ConnectionString. Set before the host builds.
        Environment.SetEnvironmentVariable("postgres__ConnectionString", _container.GetConnectionString());

        _ = Services; // force the host to build with the env var in place
        await RunMigrationsAsync();
        NpgsqlConnection.ClearAllPools();
    }

    public new async Task DisposeAsync()
    {
        await _container.DisposeAsync();
        await base.DisposeAsync();
    }

    /// <summary>Overridden per module to migrate that module's schema and start its dependencies.</summary>
    protected virtual Task RunMigrationsAsync() => Task.CompletedTask;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            foreach (var hostedService in services
                         .Where(d => d.ServiceType == typeof(IHostedService))
                         .ToList())
            {
                services.Remove(hostedService);
            }

            services.AddAuthentication(FakeAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>(FakeAuthHandler.SchemeName, _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = FakeAuthHandler.SchemeName;
                options.DefaultChallengeScheme = FakeAuthHandler.SchemeName;
            });
        });
    }
}
