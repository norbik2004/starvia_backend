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

        var existingAttachments = await postAttachmentRepository.GetByPostIdAsync(request.postId, cancellationToken);
        if (existingAttachments.Count + request.fileIds.Count > PostAttachment.MaxPerPost)
        {
            throw new TooManyPostAttachmentsException(PostAttachment.MaxPerPost);
        }

        var files = await mediaClient.GetFilesAsync(command.userId, request.fileIds, cancellationToken);

        var filesById = files.ToDictionary(file => file.Id);

        var nextOrder = existingAttachments.Count == 0 ? 0 : existingAttachments.Max(pa => pa.Order) + 1;

        var attachments = new List<PostAttachment>(request.fileIds.Count);
        foreach (var fileId in request.fileIds)
        {
            if (!filesById.TryGetValue(fileId, out var file))
            {
                throw new MediaIntegrationException($"File '{fileId}' was not found in the media API.");
            }

            attachments.Add(PostAttachment.Create(request.postId, file.Id, nextOrder++));
        }

        foreach (var attachment in attachments)
        {
            await postAttachmentRepository.AddAsync(attachment, cancellationToken);
        }

        return new AddPostAttachmentsResult(attachments.Select(a => a.Id).ToList());
    }
}
