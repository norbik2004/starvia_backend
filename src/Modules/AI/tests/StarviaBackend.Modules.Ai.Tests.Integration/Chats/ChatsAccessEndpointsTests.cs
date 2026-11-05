using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;

namespace StarviaBackend.Modules.Ai.Tests.Integration.Chats;

/// <summary>
/// Chats are private conversations: only their owner may see or change them, admins included
/// (see ChatAccessPolicy), and browsing never shows anybody else's chats.
/// </summary>
public sealed class ChatsAccessEndpointsTests(AiApp app) : AiIntegrationTest(app)
{
    // ---- admins have no back door -----------------------------------------------------------

    [Fact]
    public async Task An_admin_cannot_read_someone_elses_chat()
    {
        // Arrange
        var chatId = await CreateChatAsync(CreateUserClient());

        // Act
        var response = await CreateAdminClient().GetAsync($"{ChatsUrl}/{chatId}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.Forbidden, "chat_access_denied");
    }

    [Fact]
    public async Task An_admin_cannot_rename_archive_or_extend_someone_elses_chat()
    {
        // Arrange
        var owner = CreateUserClient();
        var chatId = await CreateChatAsync(owner, title: "Private");
        var admin = CreateAdminClient();

        // Act
        var update = await admin.PutAsJsonAsync($"{ChatsUrl}/{chatId}", new { title = "Renamed", status = ChatStatus.Archived });
        var message = await AddMessageAsync(admin, chatId, "Hello from the admin");

        // Assert
        await AssertErrorCodeAsync(update, HttpStatusCode.Forbidden, "chat_access_denied");
        await AssertErrorCodeAsync(message, HttpStatusCode.Forbidden, "chat_access_denied");
        var chat = await GetChatAsync(owner, chatId);
        chat.Title.Should().Be("Private");
        chat.Status.Should().Be(ChatStatus.Active);
        chat.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task An_admin_cannot_remove_someone_elses_chat()
    {
        // Arrange
        var owner = CreateUserClient();
        var chatId = await CreateChatAsync(owner);

        // Act
        var response = await CreateAdminClient().DeleteAsync($"{ChatsUrl}/{chatId}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.Forbidden, "chat_access_denied");
        (await GetChatAsync(owner, chatId)).Id.Should().Be(chatId);
    }

    [Fact]
    public async Task An_admin_browsing_chats_sees_only_their_own()
    {
        // Arrange
        var admin = CreateAdminClient();
        var own = await CreateChatAsync(admin, title: "Admin chat");
        await CreateChatAsync(CreateUserClient(), title: "User chat");

        // Act
        var page = await admin.GetFromJsonAsync<ChatPageResponse>($"{ChatsUrl}?pageSize=100");

        // Assert
        page!.Items.Select(c => c.Id).Should().Equal(own);
    }

    // ---- browse ------------------------------------------------------------------------------

    [Fact]
    public async Task Browse_chats_by_someone_elses_owner_id_returns_nothing()
    {
        // Arrange
        var otherId = Guid.NewGuid();
        await CreateChatAsync(CreateUserClient(otherId));

        // Act
        var page = await CreateUserClient().GetFromJsonAsync<ChatPageResponse>($"{ChatsUrl}?ownerId={otherId}");

        // Assert
        page!.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Browse_chats_exposes_the_owner_id()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        await CreateChatAsync(client);

        // Act
        var page = await client.GetFromJsonAsync<ChatPageResponse>(ChatsUrl);

        // Assert
        page!.Items.Should().ContainSingle().Which.OwnerId.Should().Be(userId);
    }

    [Fact]
    public async Task Browse_chats_searches_the_title_ignoring_case_and_treating_wildcards_literally()
    {
        // Arrange
        var client = CreateUserClient();
        var coffee = await CreateChatAsync(client, title: "Ideas about COFFEE");
        await CreateChatAsync(client, title: "Something else");
        var percent = await CreateChatAsync(client, title: "100%_done");

        // Act
        var byText = await client.GetFromJsonAsync<ChatPageResponse>($"{ChatsUrl}?search=coffee");
        var byPercent = await client.GetFromJsonAsync<ChatPageResponse>($"{ChatsUrl}?search=%25");
        var byUnderscore = await client.GetFromJsonAsync<ChatPageResponse>($"{ChatsUrl}?search=_");

        // Assert
        byText!.Items.Select(c => c.Id).Should().Equal(coffee);
        byPercent!.Items.Select(c => c.Id).Should().Equal(percent);
        byUnderscore!.Items.Select(c => c.Id).Should().Equal(percent);
    }

    [Fact]
    public async Task Browse_chats_sorts_by_title_and_by_creation()
    {
        // Arrange
        var client = CreateUserClient();
        var b = await CreateChatAsync(client, title: "Bbb");
        var a = await CreateChatAsync(client, title: "Aaa");
        var c = await CreateChatAsync(client, title: "Ccc");

        // Act
        var byTitle = await client.GetFromJsonAsync<ChatPageResponse>($"{ChatsUrl}?sortBy=Title&descending=false");
        var byTitleDescending = await client.GetFromJsonAsync<ChatPageResponse>($"{ChatsUrl}?sortBy=Title&descending=true");
        var oldestFirst = await client.GetFromJsonAsync<ChatPageResponse>($"{ChatsUrl}?sortBy=CreatedAt&descending=false");

        // Assert
        byTitle!.Items.Select(x => x.Id).Should().Equal(a, b, c);
        byTitleDescending!.Items.Select(x => x.Id).Should().Equal(c, b, a);
        oldestFirst!.Items.Select(x => x.Id).Should().Equal(b, a, c);
    }

    [Fact]
    public async Task Browse_chats_page_past_the_end_returns_empty_items_but_the_total()
    {
        // Arrange
        var client = CreateUserClient();
        await CreateChatAsync(client);

        // Act
        var page = await client.GetFromJsonAsync<ChatPageResponse>($"{ChatsUrl}?page=500");

        // Assert
        page!.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(1);
        page.Page.Should().Be(500);
    }
}
