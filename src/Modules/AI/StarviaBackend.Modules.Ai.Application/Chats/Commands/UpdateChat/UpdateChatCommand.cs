using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Ai.Application.Chats.Commands.UpdateChat;

internal sealed record UpdateChatCommand(Guid ChatId, UpdateChatRequest Request, Guid UserId) : ICommand;

/// <param name="Title">The new title of the chat.</param>
/// <param name="Status"><see cref="ChatStatus.Archived"/> archives the chat, <see cref="ChatStatus.Active"/> restores it.</param>
internal sealed record UpdateChatRequest(string Title, ChatStatus Status);
