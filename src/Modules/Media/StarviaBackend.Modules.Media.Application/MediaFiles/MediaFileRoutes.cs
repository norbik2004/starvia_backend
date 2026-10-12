namespace StarviaBackend.Modules.Media.Application.MediaFiles;

internal static class MediaFileRoutes
{
    public const string BasePath = "v1/media-files";
    public const string ContentSegment = "content";

    public static string Content(Guid fileId) => $"/{BasePath}/{fileId}/{ContentSegment}";
}
