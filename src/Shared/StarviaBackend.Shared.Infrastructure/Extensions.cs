using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using StarviaBackend.Shared.Abstractions.Time;
using StarviaBackend.Shared.Infrastructure.Auth;
using StarviaBackend.Shared.Infrastructure.Contexts;
using StarviaBackend.Shared.Infrastructure.Cqrs;
using StarviaBackend.Shared.Infrastructure.Exceptions;
using StarviaBackend.Shared.Infrastructure.Postgres;
using StarviaBackend.Shared.Infrastructure.Api;
using StarviaBackend.Shared.Infrastructure.Time;

namespace StarviaBackend.Shared.Infrastructure;

public static class Extensions
{
    private const string CorsPolicy = "cors";

    /// <summary>
    /// The shared composition root. Call after the modules have registered their own services.
    /// Modules register their handlers/DbContexts; this wires the cross-cutting plumbing.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IClock, UtcClock>();
        services.AddContext();
        services.AddErrorHandling();
        services.AddDispatchers();
        services.AddPostgres(configuration);
        services.AddAuth(configuration);

        services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

        services.AddControllers()
            .ConfigureApplicationPartManager(manager =>
                manager.FeatureProviders.Add(new InternalControllerFeatureProvider()));

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.EnableAnnotations();
            options.CustomSchemaIds(type => type.FullName);
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste the JWT access token (without the 'Bearer ' prefix).",
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                    },
                    []
                },
            });
        });

        return services;
    }

    /// <summary>The shared middleware pipeline. Call before mapping endpoints/controllers.</summary>
    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

        app.UseCors(CorsPolicy);
        app.UseCorrelationId();
        app.UseErrorHandling();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }
}
