using System.Net;
using System.Net.Http.Json;
using StarviaBackend.Modules.Media.Integrations.External;
using StarviaBackend.Shared.Infrastructure.Integrations;

namespace StarviaBackend.Modules.Media.Integrations;

/// <summary>
/// Typed HttpClient implementation of <see cref="IMediaClient"/>. Loads each file from the media API,
/// rejects the call when the requesting user does not own the file, maps the wire format into
/// <see cref="MediaFile"/>, and turns transport failures into <see cref="MediaIntegrationException"/>.
/// </summary>
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
            .OnCode("media_not_found", () => FileNotFound(fileId))
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
