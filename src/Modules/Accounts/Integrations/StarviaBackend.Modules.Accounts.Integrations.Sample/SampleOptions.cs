namespace StarviaBackend.Modules.Accounts.Integrations.Sample;

public sealed class SampleOptions
{
    public const string SectionName = "integrations:sample";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
