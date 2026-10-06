using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Posts.Api.Endpoints.Posts;

internal static class PostsEndpoint
{
    public const string BasePath = "v1/posts";
    public const string Tag = "Posts";

    public const string AdminRole = UserRoles.Admin;
}
