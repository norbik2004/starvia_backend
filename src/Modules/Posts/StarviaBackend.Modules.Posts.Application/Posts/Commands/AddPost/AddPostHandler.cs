using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Core.Posts.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Time;

namespace StarviaBackend.Modules.Posts.Application.Posts.Commands.AddPost;

internal sealed class AddPostHandler(IPostRepository postRepository, IClock clock) : ICommandHandler<AddPostCommand, AddPostResult>
{
    public async Task<AddPostResult> HandleAsync(AddPostCommand command, CancellationToken cancellationToken = default)
    {
        var post = Post.Create(command.request.Title, clock.UtcNow, command.UserId);

        await postRepository.Add
    }
}
