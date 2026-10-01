using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Posts.Core.Posts.Exceptions;
using StarviaBackend.Modules.Posts.Core.Posts.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Time;

namespace StarviaBackend.Modules.Posts.Application.Posts.Commands.UpdatePost;

internal class UpdatePostHandler(IPostRepository postRepository) : ICommandHandler<UpdatePostCommand, UpdatePostResult>
{
    public async Task<UpdatePostResult> HandleAsync(UpdatePostCommand command, CancellationToken cancellationToken = default)
    {
        var post = await postRepository.GetPostByIdAsync(command.request.PostId, cancellationToken)
            ?? throw new PostNotFoundException(command.request.PostId);

        post.Update(
            command.request.Title,
            command.request.Body,
            command.request.Status);

        await postRepository.UpdateAsync(post);

        return new UpdatePostResult(post.Id);
    }
}
