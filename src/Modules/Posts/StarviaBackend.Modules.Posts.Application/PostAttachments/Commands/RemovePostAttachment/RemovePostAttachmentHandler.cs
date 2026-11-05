using StarviaBackend.Modules.Posts.Application.Posts.Access;
using StarviaBackend.Modules.Posts.Core.Posts.Access;
using StarviaBackend.Modules.Posts.Core.Posts.Exceptions;
using StarviaBackend.Modules.Posts.Core.Posts.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.RemovePostAttachment;

internal sealed class RemovePostAttachmentHandler(
    IPostRepository postRepository,
    IPostAttachmentRepository postAttachmentRepository,
    IPostAccessService access)
    : ICommandHandler<RemovePostAttachmentCommand>
{
    public async Task HandleAsync(RemovePostAttachmentCommand command, CancellationToken cancellationToken = default)
    {
        var request = command.request;
        var post = await postRepository.GetPostByIdAsync(request.postId, cancellationToken)
            ?? throw new PostNotFoundException(request.postId);

        access.EnsureAllowed(PostAction.ManageAttachments, post);

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
