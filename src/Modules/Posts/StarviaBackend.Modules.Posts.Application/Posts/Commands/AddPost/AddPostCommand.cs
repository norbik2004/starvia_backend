using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Posts.Application.Posts.Commands.AddPost;

public sealed record AddPostCommand(AddPostRequest request, Guid UserId) : ICommand<AddPostResult>;

public sealed record AddPostRequest(string Title, IReadOnlyList<PlatformType> Platforms);
public sealed record AddPostResult(Guid PostId);
