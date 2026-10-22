using StarviaBackend.Modules.Posts.Core.Posts.Exceptions;
using StarviaBackend.Modules.Posts.Core.Posts.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.UpdatePostAttachmentOrder;

internal sealed class UpdatePostAttachmentOrderHandler(
    IPostRepository postRepository,
    IPostAttachmentRepository postAttachmentRepository)
    : ICommandHandler<UpdatePostAttachmentOrderCommand>
{
    public async Task HandleAsync(UpdatePostAttachmentOrderCommand command, CancellationToken cancellationToken = default)
    {
        var request = command.request;
        var post = await postRepository.GetPostByIdAsync(request.postId, cancellationToken)
            ?? throw new PostNotFoundException(request.postId);

        if (post.CreatedBy != command.userId.ToString())
        {
            throw new UnauthorizedAccessException("You are not authorized to reorder attachments of this post.");
        }

        var attachments = await postAttachmentRepository.GetByPostIdAsync(request.postId, cancellationToken);
        var attachmentToMove = attachments.FirstOrDefault(pa => pa.Id == request.postAttachmentId)
            ?? throw new PostAttachmentNotFoundException(request.postAttachmentId);

        if (request.order >= attachments.Count)
        {
            throw new InvalidPostAttachmentOrderException(request.order, attachments.Count);
        }

        attachments.Remove(attachmentToMove);
        attachments.Insert(request.order, attachmentToMove);

        for (var index = 0; index < attachments.Count; index++)
        {
            attachments[index].UpdateOrder(index);
        }

        await postAttachmentRepository.UpdateRangeAsync(attachments, cancellationToken);
    }
}
