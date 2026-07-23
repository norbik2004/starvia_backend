namespace StarviaBackend.Modules.Accounts.Integrations.Sample;

/// <summary>
/// The port the module depends on. The module speaks in these domain-shaped terms; it never
/// sees the external vendor's DTOs. Keep this interface in the integration project and let
/// the module reference only the abstraction it needs.
/// </summary>
public interface ISampleClient
{
    Task<SampleProfile> GetProfileAsync(string externalId, CancellationToken cancellationToken = default);
}

/// <summary>Domain-shaped result — the anti-corruption boundary's output.</summary>
public sealed record SampleProfile(string Id, string DisplayName, bool IsActive);
