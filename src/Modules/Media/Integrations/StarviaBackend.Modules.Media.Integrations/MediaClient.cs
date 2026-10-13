using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using StarviaBackend.Modules.Media.Integrations.External;
using StarviaBackend.Shared.Abstractions.Exceptions;

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
            throw await ToFailureAsync(userId, fileId, response, cancellationToken);
        }

        var payload = await response.Content.ReadFromJsonAsync<MediaFileApiResponse>(cancellationToken)
                      ?? throw new MediaIntegrationException("Media API returned an empty body.");

        if (!IsOwner(payload.CreatedBy, userId))
        {
            throw new MediaIntegrationNotOwnerException($"User is not the owner of file.");
        }

        return new MediaFile(payload.Id, payload.FileName, payload.Url, payload.CreatedAt);
    }

    private static async Task<BusinessException> ToFailureAsync(
        Guid userId,
        Guid fileId,
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new MediaIntegrationException($"File '{fileId}' was not found in the media API.");
        }

        var error = await TryReadErrorAsync(response, cancellationToken);
        if (error?.Errors.Any(item => item.Code == "media_not_found") == true)
        {
            return new MediaIntegrationException($"File '{fileId}' was not found in the media API.");
        }

        if (error?.Errors.Any(item => item.Code == "media_file_not_owned") == true)
        {
            return new MediaIntegrationNotOwnerException($"User is not the owner of file.");
        }

        return new MediaIntegrationException($"Media API returned {(int)response.StatusCode}.");
    }

    private static async Task<MediaApiErrorResponse?> TryReadErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<MediaApiErrorResponse>(cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
    }

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
