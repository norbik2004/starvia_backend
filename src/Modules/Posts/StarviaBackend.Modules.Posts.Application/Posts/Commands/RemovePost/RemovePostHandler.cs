using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using StarviaBackend.Modules.Posts.Core.Posts.Exceptions;
using StarviaBackend.Modules.Posts.Core.Posts.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Posts.Application.Posts.Commands.RemovePost;

internal sealed class RemovePostHandler(IPostRepository postRepository) : ICommandHandler<RemovePostCommand>
{
    public async Task HandleAsync(RemovePostCommand command, CancellationToken cancellationToken = default)
    {
        var post = await postRepository.GetPostByIdAsync(command.request.postId, cancellationToken)
            ?? throw new PostNotFoundException(command.request.postId);

        if (post.CreatedBy != command.UserId.ToString())
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this post.");
        }

        await postRepository.RemovePostAsync(post, cancellationToken);
    }
}
