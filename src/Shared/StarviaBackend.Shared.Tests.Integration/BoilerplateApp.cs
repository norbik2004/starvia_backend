using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ModularMonolith.Bootstrapper;
using Npgsql;
using StarviaBackend.Shared.Infrastructure.Postgres;
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

        // "__" maps to ":" in config → ConnectionStrings:Database, the key AddPostgres reads
        // (PostgresOptions.SectionName). Env vars outrank appsettings.Development.json, so this keeps
        // the tests off the developer's local database. Set before the host builds.
        Environment.SetEnvironmentVariable(
            $"ConnectionStrings__{PostgresOptions.SectionName}",
            _container.GetConnectionString());

        _ = Services; // force the host to build with the env var in place
        EnsureHostUsesContainerDatabase();
        await RunMigrationsAsync();
        NpgsqlConnection.ClearAllPools();
    }

    public new async Task DisposeAsync()
    {
        await _container.DisposeAsync();
        await base.DisposeAsync();
    }

    /// <summary>
    /// Fails fast if the host resolved a different connection string than the container's. Without
    /// this, a config-key mismatch silently points the tests (migrations, registrations, ...) at
    /// whatever database appsettings.Development.json names - i.e. the developer's local one.
    /// </summary>
    private void EnsureHostUsesContainerDatabase()
    {
        var effective = Services.GetRequiredService<IOptions<PostgresOptions>>().Value.ConnectionString;

        if (!string.Equals(effective, _container.GetConnectionString(), StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The test host is not using the Testcontainers database. Refusing to run the tests " +
                "against another database; check the connection string key set in " +
                $"{nameof(BoilerplateApp)}.{nameof(InitializeAsync)}.");
        }
    }

    /// <summary>Overridden per module to migrate that module's schema and start its dependencies.</summary>
    protected virtual Task RunMigrationsAsync() => Task.CompletedTask;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            // Swap the RabbitMQ transport for MassTransit's in-memory one (modules' consumers stay
            // registered). Otherwise the tests publish to - and consume from - the developer's
            // broker, competing with a running dev instance and leaving poison messages in its
            // *_error queues. Must come before the hosted services are stripped below so the
            // bus stays under the tests' control (see AccountsApp).
            services.AddMassTransitTestHarness();

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
