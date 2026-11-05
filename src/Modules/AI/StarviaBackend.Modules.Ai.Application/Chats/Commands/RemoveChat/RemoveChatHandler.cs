using StarviaBackend.Modules.Ai.Application.Chats.Access;
using StarviaBackend.Modules.Ai.Core.Chats.Access;
using StarviaBackend.Modules.Ai.Core.Chats.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Ai.Application.Chats.Commands.RemoveChat;

internal sealed class RemoveChatHandler(IChatRepository chatRepository, IChatAccessService access) : ICommandHandler<RemoveChatCommand>
{
    public async Task HandleAsync(RemoveChatCommand command, CancellationToken cancellationToken = default)
    {
        var chat = await chatRepository.GetAllowedAsync(access, ChatAction.Delete, command.ChatId, cancellationToken);

        await chatRepository.RemoveAsync(chat, cancellationToken);
    }
}
