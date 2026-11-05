using System.Net;
using System.Text;
using System.Text.Json;

namespace StarviaBackend.Modules.Ai.Tests.Unit.Fakes;

/// <summary>What the client put on the wire; the body is read while the request is still alive.</summary>
internal sealed record CapturedRequest(
    HttpMethod Method,
    Uri Uri,
    string? Authorization,
    string Body,
    IReadOnlyDictionary<string, string> Headers)
{
    public JsonElement Json => JsonDocument.Parse(Body).RootElement;

    /// <summary>The value of a request header, or null when it was not sent.</summary>
    public string? Header(string name) => Headers.TryGetValue(name, out var value) ? value : null;
}

/// <summary>Answers every request from a delegate and remembers what was sent.</summary>
internal sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    public List<CapturedRequest> Requests { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
        Requests.Add(new CapturedRequest(
            request.Method,
            request.RequestUri!,
            request.Headers.Authorization?.ToString(),
            body,
            request.Headers.ToDictionary(
                header => header.Key,
                header => string.Join(",", header.Value),
                StringComparer.OrdinalIgnoreCase)));

        return respond(request);
    }

    public static StubHttpMessageHandler Json(HttpStatusCode status, object body) =>
        new(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
        });

    public static StubHttpMessageHandler Text(HttpStatusCode status, string body) =>
        new(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "text/plain"),
        });

    public static StubHttpMessageHandler Throwing(Exception exception) =>
        new(_ => throw exception);
}
