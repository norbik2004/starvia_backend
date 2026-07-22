using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace StarviaBackend.Shared.Infrastructure.Postgres;

public static class Extensions
{
    /// <summary>Binds Postgres options once for the host.</summary>
    public static IServiceCollection AddPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PostgresOptions>()
            .Bind(configuration.GetSection(PostgresOptions.SectionName));
        services.TryAddScoped<AuditableEntityInterceptor>();
        return services;
    }

    /// <summary>
    /// Registers a module's <typeparamref name="TContext"/> against Postgres with the shared
    /// auditing interceptor. Each module calls this for each of its DbContexts.
    /// </summary>
    public static IServiceCollection AddPostgres<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>((sp, options) =>
        {
            var postgres = sp.GetRequiredService<IOptions<PostgresOptions>>().Value;
            options.UseNpgsql(postgres.ConnectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
        });
        return services;
    }
}
