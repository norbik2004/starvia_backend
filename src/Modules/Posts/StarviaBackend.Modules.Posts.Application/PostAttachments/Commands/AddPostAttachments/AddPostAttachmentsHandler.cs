using StarviaBackend.Modules.Media.Integrations;
using StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.AddPostAttachment;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Core.Posts.Exceptions;
using StarviaBackend.Modules.Posts.Core.Posts.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.AddPostAttachments;

internal sealed class AddPostAttachmentsHandler(
    IPostRepository postRepository,
    IPostAttachmentRepository postAttachmentRepository,
    IMediaClient mediaClient)
    : ICommandHandler<AddPostAttachmentsCommand, AddPostAttachmentsResult>
{
    public async Task<AddPostAttachmentsResult> HandleAsync(
        AddPostAttachmentsCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = command.request;
        var post = await postRepository.GetPostByIdAsync(request.postId, cancellationToken)
            ?? throw new PostNotFoundException(request.postId);

        if (post.CreatedBy != command.userId.ToString())
        {
            throw new UnauthorizedAccessException("You are not authorized to add attachments to this post.");
        }

        var files = await mediaClient.GetFilesAsync(command.userId, request.fileIds, cancellationToken);
        var filesById = files.ToDictionary(file => file.Id);

        var attachmentIds = new List<Guid>(request.fileIds.Count);
        for (var index = 0; index < request.fileIds.Count; index++)
        {
            var fileId = request.fileIds[index];
            if (!filesById.TryGetValue(fileId, out var file))
            {
                throw new MediaIntegrationException($"File '{fileId}' was not found in the media API.");
            }

            var attachment = PostAttachment.Create(request.postId, file.Id, request.order + index);
            await postAttachmentRepository.AddAsync(attachment, cancellationToken);
            attachmentIds.Add(attachment.Id);
        }

        return new AddPostAttachmentsResult(attachmentIds);
    }
}
