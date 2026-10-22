using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.UpdatePostAttachmentOrder;

public sealed record UpdatePostAttachmentOrderCommand(UpdatePostAttachmentOrderRequest request, Guid userId) : ICommand;

public sealed record UpdatePostAttachmentOrderRequest(Guid postId, Guid postAttachmentId, int order);
