namespace StarviaBackend.Modules.Platforms.Tests.Unit.Fakes;

internal sealed class FakeHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new(handler) { BaseAddress = new Uri("http://media.test/") };
}

internal sealed class FixedBytesHandler(byte[] bytes, string contentType = "image/png") : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(bytes),
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        return Task.FromResult(response);
    }
}
