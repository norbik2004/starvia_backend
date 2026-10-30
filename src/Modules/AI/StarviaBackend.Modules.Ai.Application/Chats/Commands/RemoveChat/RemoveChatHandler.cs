using StarviaBackend.Modules.Ai.Core.Chats.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Ai.Application.Chats.Commands.RemoveChat;

internal sealed class RemoveChatHandler(IChatRepository chatRepository) : ICommandHandler<RemoveChatCommand>
{
    public async Task HandleAsync(RemoveChatCommand command, CancellationToken cancellationToken = default)
    {
        var chat = await chatRepository.GetOwnedAsync(command.ChatId, command.UserId, cancellationToken);

        await chatRepository.RemoveAsync(chat, cancellationToken);
    }
}
