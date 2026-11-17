using StarviaBackend.Modules.Posts.Core.Posts.Enums;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Posts.Application.Posts.Commands.UpdatePost;

public sealed record UpdatePostCommand(UpdatePostRequest request, Guid UserId) : ICommand<UpdatePostResult>;

public sealed record UpdatePostRequest(
    Guid PostId,
    string Title,
    string Body,
    PostStatus Status,
    IReadOnlyList<PlatformType> Platforms);
public sealed record UpdatePostResult(Guid PostId);
