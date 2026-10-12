using StarviaBackend.Modules.Media.Application.MediaFiles;
using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Media.Api.Endpoints.MediaFiles;

internal static class MediaFilesEndpoint
{
    public const string BasePath = MediaFileRoutes.BasePath;
    public const string Tag = "MediaFiles";

    public const string AdminRole = UserRoles.Admin;
}
