using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Shared.Abstractions.App;

namespace StarviaBackend.Shared.Infrastructure.App;

internal static class Extensions
{
    public static IServiceCollection AddAppUrls(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(AppUrlsOptions.SectionName);
        services.AddOptions<AppUrlsOptions>().Bind(section);
        services.AddSingleton<IAppUrls, AppUrls>();
        return services;
    }
}
