namespace StarviaBackend.Modules.Platforms.Integrations;

public sealed class PlatformsOptions
{
    public const string SectionName = "integrations:platforms";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
