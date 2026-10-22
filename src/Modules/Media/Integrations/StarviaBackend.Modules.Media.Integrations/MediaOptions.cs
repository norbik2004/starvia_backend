namespace StarviaBackend.Modules.Media.Integrations;

public sealed class MediaOptions
{
    public const string SectionName = "integrations:media";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
