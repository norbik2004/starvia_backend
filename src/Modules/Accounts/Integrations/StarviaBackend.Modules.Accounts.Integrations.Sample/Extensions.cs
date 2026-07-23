using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace StarviaBackend.Modules.Accounts.Integrations.Sample;

public static class Extensions
{
    /// <summary>
    /// Registers the sample integration as a typed HttpClient behind <see cref="ISampleClient"/>.
    /// A module would call this from its own Infrastructure <c>AddInfrastructure</c>. Not wired
    /// anywhere in this boilerplate on purpose — it is the reference pattern (see README).
    /// </summary>
    public static IServiceCollection AddSampleIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SampleOptions>().Bind(configuration.GetSection(SampleOptions.SectionName));

        services.AddHttpClient<ISampleClient, SampleClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<SampleOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);
        });

        return services;
    }
}
