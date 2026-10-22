using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Exceptions;
using StarviaBackend.Shared.Tests.Integration;

namespace StarviaBackend.Modules.Media.Tests.Integration;

/// <summary>
/// All Media integration tests share one container (per collection) and run sequentially: the
/// Testcontainers connection string is passed via a process-wide env var, so parallel classes
/// would race. New test classes just derive this and belong to the same collection.
/// </summary>
[Collection(MediaCollection.Name)]
public abstract class MediaIntegrationTest : IntegrationTest<MediaApp>
{
    protected MediaIntegrationTest(MediaApp app) : base(app)
    {
        app.Storage.Reset();
    }

    protected HttpClient CreateUserClient(Guid? userId = null, string? email = null)
    {
        var client = CreateClient(userId ?? Guid.NewGuid(), UserRoles.User);
        if (email is not null)
        {
            client.DefaultRequestHeaders.Add(FakeAuthHandler.EmailHeader, email);
        }

        return client;
    }

    protected static Task<HttpResponseMessage> PostFileAsync(
        HttpClient client,
        string fileName = "photo.png",
        string contentType = "image/png",
        byte[]? content = null,
        string? description = null)
    {
        var form = new MultipartFormDataContent();

        var file = new ByteArrayContent(content ?? [1, 2, 3, 4]);
        file.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        form.Add(file, "File", fileName);

        if (description is not null)
        {
            form.Add(new StringContent(description), "Description");
        }

        return client.PostAsync("/v1/media-files", form);
    }

    protected static async Task<AddFileResponse> UploadAsync(
        HttpClient client,
        string fileName = "photo.png",
        string contentType = "image/png",
        byte[]? content = null,
        string? description = null)
    {
        var response = await PostFileAsync(client, fileName, contentType, content, description);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<AddFileResponse>())!;
    }

    protected static async Task<FileResponse> GetFileAsync(HttpClient client, Guid fileId)
    {
        var response = await client.GetAsync($"/v1/media-files/{fileId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<FileResponse>())!;
    }

    protected static Task<HttpResponseMessage> DeleteWithBodyAsync(HttpClient client, string url, object body) =>
        client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url) { Content = JsonContent.Create(body) });

    protected static async Task AssertErrorCodeAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        response.StatusCode.Should().Be(status);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Errors.Should().Contain(e => e.Code == code);
    }

    protected sealed record AddFileResponse(Guid FileId, string FileName, string FilePath);

    protected sealed record FileResponse(
        Guid Id,
        string FileName,
        string? Description,
        int Source,
        int Type,
        string Url,
        DateTime CreatedAt,
        string? CreatedBy);

    protected sealed record FileSummaryResponse(
        Guid Id,
        string FileName,
        string? Description,
        int Source,
        int Type,
        string Url,
        DateTime CreatedAt);

    protected sealed record PagedFilesResponse(List<FileSummaryResponse> Items, int Page, int PageSize, long TotalCount);
}

[CollectionDefinition(MediaCollection.Name)]
public sealed class MediaCollection : ICollectionFixture<MediaApp>
{
    public const string Name = "MediaIntegration";
}
