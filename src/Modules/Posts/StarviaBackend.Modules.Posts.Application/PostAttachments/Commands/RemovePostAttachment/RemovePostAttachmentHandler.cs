using StarviaBackend.Modules.Posts.Core.Posts.Exceptions;
using StarviaBackend.Modules.Posts.Core.Posts.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.RemovePostAttachment;

internal sealed class RemovePostAttachmentHandler(
    IPostRepository postRepository,
    IPostAttachmentRepository postAttachmentRepository)
    : ICommandHandler<RemovePostAttachmentCommand>
{
    public async Task HandleAsync(RemovePostAttachmentCommand command, CancellationToken cancellationToken = default)
    {
        var request = command.request;
        var post = await postRepository.GetPostByIdAsync(request.postId, cancellationToken)
            ?? throw new PostNotFoundException(request.postId);

        if (post.CreatedBy != command.userId.ToString())
        {
            throw new UnauthorizedAccessException("You are not authorized to remove attachments from this post.");
        }

        var attachments = await postAttachmentRepository.GetByPostIdAsync(request.postId, cancellationToken);
        var attachmentToRemove = attachments.FirstOrDefault(pa => pa.Id == request.postAttachmentId)
            ?? throw new PostAttachmentNotFoundException(request.postAttachmentId);

        attachments.Remove(attachmentToRemove);

        for (var index = 0; index < attachments.Count; index++)
        {
            attachments[index].UpdateOrder(index);
        }

        await postAttachmentRepository.RemoveAsync(attachmentToRemove, cancellationToken);
    }
}
