using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Ai.Application.Chats.Commands.RemoveChat;

internal sealed record RemoveChatCommand(Guid ChatId, Guid UserId) : ICommand;
