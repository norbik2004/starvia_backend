using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.RemovePostAttachment;

public sealed record RemovePostAttachmentCommand(RemovePostAttachmentRequest request, Guid userId) : ICommand;

public sealed record RemovePostAttachmentRequest(Guid postId, Guid postAttachmentId);
