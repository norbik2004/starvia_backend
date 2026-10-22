using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StarviaBackend.Modules.Media.Core.Media.Storage;

namespace StarviaBackend.Modules.Media.Infrastructure.Storage.RustFs;

internal static class Extensions
{
    public static IServiceCollection AddRustFs(this IServiceCollection services)
    {
        services.AddOptions<RustFsOptions>()
            .Configure<IConfiguration>((options, configuration) =>
                configuration.GetSection(RustFsOptions.SectionName).Bind(options));

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RustFsOptions>>().Value;

            var config = new AmazonS3Config
            {
                ServiceURL = options.Endpoint,
                ForcePathStyle = options.ForcePathStyle,
                AuthenticationRegion = options.Region,
                // S3-compatible stores do not accept the optional checksums newer SDKs add by default.
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED
            };

            return new AmazonS3Client(new BasicAWSCredentials(options.AccessKey, options.SecretKey), config);
        });

        services.AddSingleton<IFileStorageService, RustFsFileStorageService>();

        return services;
    }
}
