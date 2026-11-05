using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Ai.Application.Chats.Commands.AddChat;

internal sealed record AddChatCommand(AddChatRequest Request, Guid UserId) : ICommand<AddChatResult>;

internal sealed record AddChatRequest(ChatType Type, string Title);

internal sealed record AddChatResult(Guid ChatId);
