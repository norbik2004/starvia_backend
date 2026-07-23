using System.Text.Json.Serialization;

namespace StarviaBackend.Modules.Accounts.Integrations.Sample.External;

/// <summary>
/// The vendor's wire format. Lives here and stays internal so it never leaks into the module.
/// Mapping to <see cref="SampleProfile"/> happens in <see cref="SampleClient"/>.
/// </summary>
internal sealed record SampleApiResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("full_name")]
    public string FullName { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;
}
