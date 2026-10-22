using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.AddPostAttachment;

internal sealed record AddPostAttachmentsCommand(AddPostAttachmentsRequest request, Guid userId) : ICommand<AddPostAttachmentsResult>;

public sealed record AddPostAttachmentsResult(List<Guid> postAttachmentIds);

public sealed record AddPostAttachmentsRequest(Guid postId, List<Guid> fileIds);
