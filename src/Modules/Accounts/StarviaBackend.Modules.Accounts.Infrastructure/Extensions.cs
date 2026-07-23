using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Core.Users.Repositories;
using StarviaBackend.Shared.Infrastructure.Cqrs;
using StarviaBackend.Shared.Infrastructure.Postgres;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Contexts;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Initializers;
using StarviaBackend.Modules.Accounts.Infrastructure.EF.Users.Repositories;

namespace StarviaBackend.Modules.Accounts.Infrastructure;

internal static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddPostgres<AccountsWriteDbContext>();
        services.AddPostgres<AccountsReadDbContext>();

        services.AddIdentityCore<User>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<AccountsWriteDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddHostedService<AccountsDataInitializer>();

        services.RegisterHandlers(Assembly.GetExecutingAssembly());
        return services;
    }
}
