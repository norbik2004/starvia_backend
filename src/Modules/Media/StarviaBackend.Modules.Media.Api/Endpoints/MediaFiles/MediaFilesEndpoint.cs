using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Media.Api.Endpoints.MediaFiles;

internal static class MediaFilesEndpoint
{
    public const string BasePath = "v1/media-files";
    public const string Tag = "MediaFiles";

    public const string AdminRole = UserRoles.Admin;
}
