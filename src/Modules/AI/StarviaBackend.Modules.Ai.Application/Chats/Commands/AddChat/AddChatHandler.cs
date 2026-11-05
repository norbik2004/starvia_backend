using StarviaBackend.Modules.Ai.Core.Chats.Entities;
using StarviaBackend.Modules.Ai.Core.Chats.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Ai.Application.Chats.Commands.AddChat;

internal sealed class AddChatHandler(IChatRepository chatRepository)
    : ICommandHandler<AddChatCommand, AddChatResult>
{
    public async Task<AddChatResult> HandleAsync(AddChatCommand command, CancellationToken cancellationToken = default)
    {
        var chat = Chat.Create(command.UserId, command.Request.Type, command.Request.Title);

        await chatRepository.AddAsync(chat, cancellationToken);

        return new AddChatResult(chat.Id);
    }
}
