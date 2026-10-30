using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Core.Chats.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Ai.Application.Chats.Commands.UpdateChat;

internal sealed class UpdateChatHandler(IChatRepository chatRepository) : ICommandHandler<UpdateChatCommand>
{
    public async Task HandleAsync(UpdateChatCommand command, CancellationToken cancellationToken = default)
    {
        var chat = await chatRepository.GetOwnedAsync(command.ChatId, command.UserId, cancellationToken);

        chat.Rename(command.Request.Title);

        if (command.Request.Status == ChatStatus.Archived)
        {
            chat.Archive();
        }
        else
        {
            chat.Unarchive();
        }

        await chatRepository.UpdateAsync(chat, cancellationToken);
    }
}
