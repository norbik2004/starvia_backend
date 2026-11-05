using FluentAssertions;
using StarviaBackend.Modules.Ai.Application.Chats.Commands.RemoveChat;
using StarviaBackend.Modules.Ai.Application.Chats.Commands.UpdateChat;
using StarviaBackend.Modules.Ai.Core.Chats.Access;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Core.Chats.Exceptions;
using StarviaBackend.Modules.Ai.Tests.Unit.Fakes;
using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Ai.Tests.Unit.Access;

public sealed class ChatAccessPolicyTests
{
    private static ChatAction[] AllActions => Enum.GetValues<ChatAction>();

    [Fact]
    public void ScopeFor_APlainUser_CoversOnlyTheirOwnChatsForEveryAction()
    {
        var userId = Guid.NewGuid();

        foreach (var action in AllActions)
        {
            var scope = ChatAccessPolicy.ScopeFor(action, new AccessActor(userId, IsAdmin: false));

            scope.Allows(userId.ToString()).Should().BeTrue($"owners may {action}");
            scope.Allows(Guid.NewGuid().ToString()).Should().BeFalse($"strangers may not {action}");
            scope.Allows(null).Should().BeFalse();
        }
    }

    [Fact]
    public void ScopeFor_AnAdmin_DoesNotGetToSeeOrChangeOtherUsersChats()
    {
        // Chats are private conversations: the policy is owner-only even for admins.
        var adminId = Guid.NewGuid();

        foreach (var action in AllActions)
        {
            var scope = ChatAccessPolicy.ScopeFor(action, new AccessActor(adminId, IsAdmin: true));

            scope.Unrestricted.Should().BeFalse($"admins have no blanket {action} access to chats");
            scope.Allows(adminId.ToString()).Should().BeTrue();
            scope.Allows(Guid.NewGuid().ToString()).Should().BeFalse();
        }
    }

    [Fact]
    public void ScopeFor_AnAnonymousCaller_CoversNothingForEveryAction()
    {
        foreach (var action in AllActions)
        {
            ChatAccessPolicy.ScopeFor(action, AccessActor.Anonymous).IsEmpty.Should().BeTrue($"anonymous callers may not {action}");
        }
    }
}

public sealed class ChatAccessServiceTests
{
    [Fact]
    public void EnsureAllowed_ForTheOwner_DoesNotThrow()
    {
        var owner = Guid.NewGuid();

        var act = () => TestAccess.For(owner).EnsureAllowed(ChatAction.View, Guid.NewGuid(), owner);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureAllowed_ForAnotherUserAnAdminOrAnonymous_ThrowsAccessDenied()
    {
        var chatId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var asStranger = () => TestAccess.For(Guid.NewGuid()).EnsureAllowed(ChatAction.View, chatId, ownerId);
        var asAdmin = () => TestAccess.Admin().EnsureAllowed(ChatAction.View, chatId, ownerId);
        var asAnonymous = () => TestAccess.Anonymous().EnsureAllowed(ChatAction.View, chatId, ownerId);

        asStranger.Should().Throw<ChatAccessDeniedException>().Which.Code.Should().Be("chat_access_denied");
        asAdmin.Should().Throw<ChatAccessDeniedException>();
        asAnonymous.Should().Throw<ChatAccessDeniedException>();
    }
}

public sealed class ChatHandlersAdminAccessTests
{
    private readonly InMemoryChatRepository _chats = new();
    private readonly Guid _ownerId = Guid.NewGuid();

    [Fact]
    public async Task UpdateChat_ByAnAdmin_IsDeniedAndChangesNothing()
    {
        // Arrange
        var chat = _chats.Seed(_ownerId, title: "Private");
        var handler = new UpdateChatHandler(_chats, TestAccess.Admin());
        var command = new UpdateChatCommand(chat.Id, new UpdateChatRequest("Renamed", ChatStatus.Archived), Guid.NewGuid());

        // Act
        var act = () => handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ChatAccessDeniedException>();
        chat.Title.Should().Be("Private");
        chat.Status.Should().Be(ChatStatus.Active);
    }

    [Fact]
    public async Task RemoveChat_ByAnAdmin_IsDeniedAndKeepsTheChat()
    {
        // Arrange
        var chat = _chats.Seed(_ownerId);
        var handler = new RemoveChatHandler(_chats, TestAccess.Admin());

        // Act
        var act = () => handler.HandleAsync(new RemoveChatCommand(chat.Id, Guid.NewGuid()));

        // Assert
        await act.Should().ThrowAsync<ChatAccessDeniedException>();
        _chats.Chats.Should().ContainSingle();
    }
}
