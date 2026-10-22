namespace StarviaBackend.Shared.Infrastructure.App;

public sealed class AppUrlsOptions
{
    public const string SectionName = "app";

    public string ApiBaseUrl { get; set; } = "http://localhost:5080";
    public string FrontendBaseUrl { get; set; } = "http://localhost:4200";
}
