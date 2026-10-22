using System.Net;
using System.Text;
using System.Text.Json;

namespace StarviaBackend.Modules.Posts.Tests.Unit.Fakes;

/// <summary>Answers every request from a delegate and remembers what was requested.</summary>
internal sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    public List<Uri> RequestedUris { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestedUris.Add(request.RequestUri!);
        return Task.FromResult(respond(request));
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
