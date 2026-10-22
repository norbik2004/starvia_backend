using Microsoft.Extensions.Options;
using StarviaBackend.Shared.Abstractions.App;

namespace StarviaBackend.Shared.Infrastructure.App;

internal sealed class AppUrls(IOptions<AppUrlsOptions> options) : IAppUrls
{
    private readonly AppUrlsOptions _options = options.Value;

    public string ApiBaseUrl => Trim(_options.ApiBaseUrl);
    public string FrontendBaseUrl => Trim(_options.FrontendBaseUrl);

    private static string Trim(string value) => string.IsNullOrWhiteSpace(value)
        ? string.Empty
        : value.TrimEnd('/');
}
