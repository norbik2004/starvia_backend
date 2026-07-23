using System.Net;
using System.Net.Http.Json;
using StarviaBackend.Modules.Accounts.Integrations.Sample.External;

namespace StarviaBackend.Modules.Accounts.Integrations.Sample;

/// <summary>
/// Typed HttpClient implementation of <see cref="ISampleClient"/>. Owns the wire format,
/// translates it into the domain shape, and turns transport failures into a domain exception.
/// </summary>
internal sealed class SampleClient(HttpClient httpClient) : ISampleClient
{
    public async Task<SampleProfile> GetProfileAsync(string externalId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync($"profiles/{externalId}", cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new SampleIntegrationException($"Sample API unreachable: {ex.Message}");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new SampleIntegrationException($"Profile '{externalId}' was not found in the sample API.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new SampleIntegrationException($"Sample API returned {(int)response.StatusCode}.");
        }

        var payload = await response.Content.ReadFromJsonAsync<SampleApiResponse>(cancellationToken)
                      ?? throw new SampleIntegrationException("Sample API returned an empty body.");

        return Map(payload);
    }

    private static SampleProfile Map(SampleApiResponse response) =>
        new(response.Id, response.FullName, response.Status.Equals("active", StringComparison.OrdinalIgnoreCase));
}
