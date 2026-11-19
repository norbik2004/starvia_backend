using System.Net;
using System.Net.Http.Json;
using StarviaBackend.Modules.Media.Integrations.External;
using StarviaBackend.Shared.Infrastructure.Integrations;

namespace StarviaBackend.Modules.Media.Integrations;


internal sealed class MediaClient(HttpClient httpClient) : IMediaClient
{
    public async Task<IReadOnlyList<MediaFile>> GetFilesAsync(
        Guid userId,
        IEnumerable<Guid> fileIds,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new MediaIntegrationException("User id cannot be empty.");
        }

        var files = new List<MediaFile>();

        foreach (var fileId in fileIds.Distinct())
        {
            if (fileId == Guid.Empty)
            {
                throw new MediaIntegrationException("File id cannot be empty.");
            }

            files.Add(await GetFileAsync(userId, fileId, cancellationToken));
        }

        return files;
    }

    public async Task<MediaFileContent> DownloadContentAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        if (fileId == Guid.Empty)
        {
            throw new MediaIntegrationException("File id cannot be empty.");
        }

        using var response = await SendAsync($"v1/media-files/{fileId}/content", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await response.ToFailureAsync(FailureRules(fileId), cancellationToken);
        }

        var stream = new MemoryStream();
        await response.Content.CopyToAsync(stream, cancellationToken);
        stream.Position = 0;

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"');

        return new MediaFileContent(stream, contentType, fileName);
    }

    private async Task<MediaFile> GetFileAsync(Guid userId, Guid fileId, CancellationToken cancellationToken)
    {
        using var response = await SendAsync($"v1/media-files/{fileId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await response.ToFailureAsync(FailureRules(fileId), cancellationToken);
        }

        var payload = await response.Content.ReadFromJsonAsync<MediaFileApiResponse>(cancellationToken)
                      ?? throw new MediaIntegrationException("Media API returned an empty body.");

        if (!IsOwner(payload.CreatedBy, userId))
        {
            throw new MediaIntegrationNotOwnerException($"User is not the owner of file.");
        }

        return new MediaFile(payload.Id, payload.FileName, payload.Url, payload.CreatedAt);
    }

    private static IntegrationFailureRules FailureRules(Guid fileId) =>
        new IntegrationFailureRules(status => new MediaIntegrationException($"Media API returned {(int)status}."))
            .OnStatus(HttpStatusCode.NotFound, () => FileNotFound(fileId))
            .OnStatus(HttpStatusCode.Unauthorized, () => new MediaIntegrationException(
                $"Media API returned 401 when downloading file '{fileId}'. Authorization was not forwarded."))
            .OnCode("media_not_found", () => FileNotFound(fileId))
            .OnCode("media_access_denied", () => new MediaIntegrationNotOwnerException("User is not the owner of file."))
            .OnCode("media_file_not_owned", () => new MediaIntegrationNotOwnerException("User is not the owner of file."));

    private static MediaIntegrationException FileNotFound(Guid fileId) =>
        new($"File '{fileId}' was not found in the media API.");

    private static bool IsOwner(string? createdBy, Guid userId) =>
        Guid.TryParse(createdBy, out var ownerId) && ownerId == userId;

    private async Task<HttpResponseMessage> SendAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.GetAsync(path, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException
                                   || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            throw new MediaIntegrationException($"Media API unreachable: {ex.Message}");
        }
    }
}
