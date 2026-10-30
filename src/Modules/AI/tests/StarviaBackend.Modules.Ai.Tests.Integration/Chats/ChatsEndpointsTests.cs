using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Infrastructure.EF.Contexts;

namespace StarviaBackend.Modules.Ai.Tests.Integration.Chats;

public sealed class ChatsEndpointsTests(AiApp app) : AiIntegrationTest(app)
{
    // --- add ---------------------------------------------------------------------------------

    [Theory]
    [InlineData(ChatType.AskAgent)]
    [InlineData(ChatType.PostContentGeneration)]
    public async Task Add_chat_returns_created_and_the_user_can_read_it_back(ChatType type)
    {
        // Arrange
        var client = CreateUserClient();

        // Act
        var chatId = await CreateChatAsync(client, type, "  Ideas for December  ");

        // Assert
        var chat = await GetChatAsync(client, chatId);
        chat.Id.Should().Be(chatId);
        chat.Type.Should().Be(type);
        chat.Title.Should().Be("Ideas for December");
        chat.Status.Should().Be(ChatStatus.Active);
        chat.PostId.Should().BeNull();
        chat.LastMessageAt.Should().BeNull();
        chat.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task Add_chat_twice_creates_two_independent_chats()
    {
        // Arrange
        var client = CreateUserClient();

        // Act
        var first = await CreateChatAsync(client, title: "Same title");
        var second = await CreateChatAsync(client, title: "Same title");

        // Assert
        first.Should().NotBe(second);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Add_chat_with_blank_title_returns_bad_request(string title)
    {
        // Act
        var response = await Client.PostAsJsonAsync(ChatsUrl, new { type = ChatType.AskAgent, title });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Add_chat_with_too_long_title_returns_bad_request()
    {
        // Act
        var response = await Client.PostAsJsonAsync(ChatsUrl, new { type = ChatType.AskAgent, title = new string('t', 151) });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    public async Task Add_chat_with_unknown_type_returns_bad_request(int type)
    {
        // Act
        var response = await Client.PostAsJsonAsync(ChatsUrl, new { type, title = "Title" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Add_chat_without_identity_returns_unauthorized()
    {
        // Arrange
        var anonymous = App.CreateClient();

        // Act
        var response = await anonymous.PostAsJsonAsync(ChatsUrl, new { type = ChatType.AskAgent, title = "Anonymous" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- get ---------------------------------------------------------------------------------

    [Fact]
    public async Task Get_unknown_chat_returns_chat_not_found()
    {
        // Act
        var response = await Client.GetAsync($"{ChatsUrl}/{Guid.NewGuid()}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "chat_not_found");
    }

    [Fact]
    public async Task Get_chat_of_another_user_returns_chat_not_owned()
    {
        // Arrange
        var chatId = await CreateChatAsync(CreateUserClient());

        // Act
        var response = await CreateUserClient().GetAsync($"{ChatsUrl}/{chatId}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "chat_not_owned");
    }

    [Fact]
    public async Task Get_chat_without_identity_returns_unauthorized()
    {
        // Act
        var response = await App.CreateClient().GetAsync($"{ChatsUrl}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_chat_with_a_route_that_is_not_a_guid_returns_not_found()
    {
        // Act
        var response = await Client.GetAsync($"{ChatsUrl}/not-a-guid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- messages ----------------------------------------------------------------------------

    [Fact]
    public async Task Add_message_stores_the_prompt_verbatim_and_returns_the_sequence()
    {
        // Arrange
        var client = CreateUserClient();
        var chatId = await CreateChatAsync(client);

        // Act
        var response = await AddMessageAsync(client, chatId, "  Suggest a post about coffee  ");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<MessageIdResponse>();
        created!.Sequence.Should().Be(1);

        var chat = await GetChatAsync(client, chatId);
        var message = chat.Messages.Should().ContainSingle().Subject;
        message.Id.Should().Be(created.MessageId);
        message.Sequence.Should().Be(1);
        message.Role.Should().Be(ChatMessageRole.User);
        message.Content.Should().Be("  Suggest a post about coffee  ");
        message.Model.Should().BeNull();
        message.InputTokens.Should().BeNull();
        message.OutputTokens.Should().BeNull();
        chat.LastMessageAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Add_message_keeps_the_conversation_in_order()
    {
        // Arrange
        var client = CreateUserClient();
        var chatId = await CreateChatAsync(client);

        // Act
        foreach (var content in new[] { "First", "Second", "Third" })
        {
            (await AddMessageAsync(client, chatId, content)).StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // Assert
        var chat = await GetChatAsync(client, chatId);
        chat.Messages.Select(m => m.Content).Should().Equal("First", "Second", "Third");
        chat.Messages.Select(m => m.Sequence).Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task Add_message_to_an_unknown_chat_returns_chat_not_found()
    {
        // Act
        var response = await AddMessageAsync(Client, Guid.NewGuid(), "Hello");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "chat_not_found");
    }

    [Fact]
    public async Task Add_message_to_a_chat_of_another_user_returns_chat_not_owned_and_stores_nothing()
    {
        // Arrange
        var owner = CreateUserClient();
        var chatId = await CreateChatAsync(owner);

        // Act
        var response = await AddMessageAsync(CreateUserClient(), chatId, "Intruder");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "chat_not_owned");
        (await GetChatAsync(owner, chatId)).Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task Add_message_to_an_archived_chat_returns_chat_archived()
    {
        // Arrange
        var client = CreateUserClient();
        var chatId = await CreateChatAsync(client);
        (await client.PutAsJsonAsync($"{ChatsUrl}/{chatId}", new { title = "Done", status = ChatStatus.Archived }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act
        var response = await AddMessageAsync(client, chatId, "Too late");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "chat_archived");
        (await GetChatAsync(client, chatId)).Messages.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Add_message_with_blank_content_returns_bad_request(string content)
    {
        // Arrange
        var client = CreateUserClient();
        var chatId = await CreateChatAsync(client);

        // Act
        var response = await AddMessageAsync(client, chatId, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Add_message_accepts_the_maximum_length_and_rejects_longer_content()
    {
        // Arrange
        var client = CreateUserClient();
        var chatId = await CreateChatAsync(client);

        // Act
        var atLimit = await AddMessageAsync(client, chatId, new string('a', 8000));
        var overLimit = await AddMessageAsync(client, chatId, new string('a', 8001));

        // Assert
        atLimit.StatusCode.Should().Be(HttpStatusCode.Created);
        overLimit.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await GetChatAsync(client, chatId)).Messages.Should().ContainSingle();
    }

    [Fact]
    public async Task Add_message_without_identity_returns_unauthorized()
    {
        // Act
        var response = await AddMessageAsync(App.CreateClient(), Guid.NewGuid(), "Hello");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- update ------------------------------------------------------------------------------

    [Fact]
    public async Task Update_chat_renames_it()
    {
        // Arrange
        var client = CreateUserClient();
        var chatId = await CreateChatAsync(client, title: "Old title");

        // Act
        var response = await client.PutAsJsonAsync($"{ChatsUrl}/{chatId}", new { title = " New title ", status = ChatStatus.Active });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var chat = await GetChatAsync(client, chatId);
        chat.Title.Should().Be("New title");
        chat.Status.Should().Be(ChatStatus.Active);
    }

    [Fact]
    public async Task Update_chat_can_archive_it_and_restore_it()
    {
        // Arrange
        var client = CreateUserClient();
        var chatId = await CreateChatAsync(client, title: "Ideas");

        // Act & Assert
        (await client.PutAsJsonAsync($"{ChatsUrl}/{chatId}", new { title = "Ideas", status = ChatStatus.Archived }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await GetChatAsync(client, chatId)).Status.Should().Be(ChatStatus.Archived);

        (await client.PutAsJsonAsync($"{ChatsUrl}/{chatId}", new { title = "Ideas", status = ChatStatus.Active }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await GetChatAsync(client, chatId)).Status.Should().Be(ChatStatus.Active);
    }

    [Fact]
    public async Task Update_chat_keeps_its_history()
    {
        // Arrange
        var client = CreateUserClient();
        var chatId = await CreateChatAsync(client);
        await AddMessageAsync(client, chatId, "Remember me");

        // Act
        var response = await client.PutAsJsonAsync($"{ChatsUrl}/{chatId}", new { title = "Renamed", status = ChatStatus.Archived });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var chat = await GetChatAsync(client, chatId);
        chat.Messages.Should().ContainSingle().Which.Content.Should().Be("Remember me");
    }

    [Fact]
    public async Task Update_unknown_chat_returns_chat_not_found()
    {
        // Act
        var response = await Client.PutAsJsonAsync($"{ChatsUrl}/{Guid.NewGuid()}", new { title = "Title", status = ChatStatus.Active });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "chat_not_found");
    }

    [Fact]
    public async Task Update_chat_of_another_user_returns_chat_not_owned_and_changes_nothing()
    {
        // Arrange
        var owner = CreateUserClient();
        var chatId = await CreateChatAsync(owner, title: "Private");

        // Act
        var response = await CreateUserClient().PutAsJsonAsync(
            $"{ChatsUrl}/{chatId}",
            new { title = "Hijacked", status = ChatStatus.Archived });

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "chat_not_owned");
        var chat = await GetChatAsync(owner, chatId);
        chat.Title.Should().Be("Private");
        chat.Status.Should().Be(ChatStatus.Active);
    }

    [Fact]
    public async Task Update_chat_with_blank_title_returns_bad_request()
    {
        // Arrange
        var client = CreateUserClient();
        var chatId = await CreateChatAsync(client, title: "Keep me");

        // Act
        var response = await client.PutAsJsonAsync($"{ChatsUrl}/{chatId}", new { title = " ", status = ChatStatus.Active });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await GetChatAsync(client, chatId)).Title.Should().Be("Keep me");
    }

    [Fact]
    public async Task Update_chat_with_unknown_status_returns_bad_request()
    {
        // Arrange
        var client = CreateUserClient();
        var chatId = await CreateChatAsync(client);

        // Act
        var response = await client.PutAsJsonAsync($"{ChatsUrl}/{chatId}", new { title = "Title", status = 99 });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_chat_with_empty_id_returns_bad_request()
    {
        // Act
        var response = await Client.PutAsJsonAsync($"{ChatsUrl}/{Guid.Empty}", new { title = "Title", status = ChatStatus.Active });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_chat_without_identity_returns_unauthorized()
    {
        // Act
        var response = await App.CreateClient().PutAsJsonAsync(
            $"{ChatsUrl}/{Guid.NewGuid()}",
            new { title = "Title", status = ChatStatus.Active });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- remove ------------------------------------------------------------------------------

    [Fact]
    public async Task Remove_chat_deletes_it()
    {
        // Arrange
        var client = CreateUserClient();
        var chatId = await CreateChatAsync(client);

        // Act
        var response = await client.DeleteAsync($"{ChatsUrl}/{chatId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var afterwards = await client.GetAsync($"{ChatsUrl}/{chatId}");
        await AssertErrorCodeAsync(afterwards, HttpStatusCode.BadRequest, "chat_not_found");
    }

    [Fact]
    public async Task Remove_chat_also_deletes_its_messages()
    {
        // Arrange
        var client = CreateUserClient();
        var chatId = await CreateChatAsync(client);
        await AddMessageAsync(client, chatId, "First");
        await AddMessageAsync(client, chatId, "Second");
        var before = await WithDbAsync<AiWriteDbContext, int>(db => db.ChatMessages.CountAsync(m => m.ChatId == chatId));
        before.Should().Be(2);

        // Act
        var response = await client.DeleteAsync($"{ChatsUrl}/{chatId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var remaining = await WithDbAsync<AiWriteDbContext, int>(db => db.ChatMessages.CountAsync(m => m.ChatId == chatId));
        remaining.Should().Be(0);
    }

    [Fact]
    public async Task Remove_chat_leaves_other_chats_untouched()
    {
        // Arrange
        var client = CreateUserClient();
        var toRemove = await CreateChatAsync(client, title: "Remove me");
        var toKeep = await CreateChatAsync(client, title: "Keep me");
        await AddMessageAsync(client, toKeep, "Still here");

        // Act
        await client.DeleteAsync($"{ChatsUrl}/{toRemove}");

        // Assert
        var chat = await GetChatAsync(client, toKeep);
        chat.Title.Should().Be("Keep me");
        chat.Messages.Should().ContainSingle();
    }

    [Fact]
    public async Task Remove_unknown_chat_returns_chat_not_found()
    {
        // Act
        var response = await Client.DeleteAsync($"{ChatsUrl}/{Guid.NewGuid()}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "chat_not_found");
    }

    [Fact]
    public async Task Remove_chat_of_another_user_returns_chat_not_owned_and_keeps_the_chat()
    {
        // Arrange
        var owner = CreateUserClient();
        var chatId = await CreateChatAsync(owner);

        // Act
        var response = await CreateUserClient().DeleteAsync($"{ChatsUrl}/{chatId}");

        // Assert
        await AssertErrorCodeAsync(response, HttpStatusCode.BadRequest, "chat_not_owned");
        (await GetChatAsync(owner, chatId)).Id.Should().Be(chatId);
    }

    [Fact]
    public async Task Remove_chat_with_empty_id_returns_bad_request()
    {
        // Act
        var response = await Client.DeleteAsync($"{ChatsUrl}/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Remove_chat_without_identity_returns_unauthorized()
    {
        // Act
        var response = await App.CreateClient().DeleteAsync($"{ChatsUrl}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- browse ------------------------------------------------------------------------------

    [Fact]
    public async Task Browse_chats_when_the_user_has_none_returns_an_empty_page()
    {
        // Act
        var page = await CreateUserClient().GetFromJsonAsync<ChatPageResponse>(ChatsUrl);

        // Assert
        page!.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(0);
        page.Page.Should().Be(1);
        page.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Browse_chats_returns_only_the_chats_of_the_caller()
    {
        // Arrange
        var me = CreateUserClient();
        var other = CreateUserClient();
        var mine = await CreateChatAsync(me, title: "Mine");
        await CreateChatAsync(other, title: "Not mine");

        // Act
        var page = await me.GetFromJsonAsync<ChatPageResponse>(ChatsUrl);

        // Assert
        page!.TotalCount.Should().Be(1);
        page.Items.Should().ContainSingle().Which.Id.Should().Be(mine);
    }

    [Fact]
    public async Task Browse_chats_orders_by_the_latest_activity()
    {
        // Arrange
        var client = CreateUserClient();
        var oldest = await CreateChatAsync(client, title: "Oldest");
        var middle = await CreateChatAsync(client, title: "Middle");
        var newest = await CreateChatAsync(client, title: "Newest");

        // Act
        var beforeActivity = await client.GetFromJsonAsync<ChatPageResponse>(ChatsUrl);
        await AddMessageAsync(client, oldest, "Bump");
        var afterActivity = await client.GetFromJsonAsync<ChatPageResponse>(ChatsUrl);

        // Assert
        beforeActivity!.Items.Select(c => c.Id).Should().Equal(newest, middle, oldest);
        afterActivity!.Items.Select(c => c.Id).Should().Equal(oldest, newest, middle);
        afterActivity.Items[0].LastMessageAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Browse_chats_filters_by_type()
    {
        // Arrange
        var client = CreateUserClient();
        await CreateChatAsync(client, ChatType.AskAgent, "Helper");
        var generator = await CreateChatAsync(client, ChatType.PostContentGeneration, "Generator");

        // Act
        var page = await client.GetFromJsonAsync<ChatPageResponse>($"{ChatsUrl}?type={(int)ChatType.PostContentGeneration}");

        // Assert
        page!.TotalCount.Should().Be(1);
        page.Items.Should().ContainSingle().Which.Id.Should().Be(generator);
    }

    [Fact]
    public async Task Browse_chats_filters_by_status()
    {
        // Arrange
        var client = CreateUserClient();
        var active = await CreateChatAsync(client, title: "Active");
        var archived = await CreateChatAsync(client, title: "Archived");
        await client.PutAsJsonAsync($"{ChatsUrl}/{archived}", new { title = "Archived", status = ChatStatus.Archived });

        // Act
        var activePage = await client.GetFromJsonAsync<ChatPageResponse>($"{ChatsUrl}?status={(int)ChatStatus.Active}");
        var archivedPage = await client.GetFromJsonAsync<ChatPageResponse>($"{ChatsUrl}?status={(int)ChatStatus.Archived}");
        var allPage = await client.GetFromJsonAsync<ChatPageResponse>(ChatsUrl);

        // Assert
        activePage!.Items.Should().ContainSingle().Which.Id.Should().Be(active);
        archivedPage!.Items.Should().ContainSingle().Which.Id.Should().Be(archived);
        allPage!.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Browse_chats_pages_through_the_history()
    {
        // Arrange
        var client = CreateUserClient();
        var ids = new List<Guid>();
        for (var i = 1; i <= 5; i++)
        {
            ids.Add(await CreateChatAsync(client, title: $"Chat {i}"));
        }

        // Act
        var first = await client.GetFromJsonAsync<ChatPageResponse>($"{ChatsUrl}?page=1&pageSize=2");
        var last = await client.GetFromJsonAsync<ChatPageResponse>($"{ChatsUrl}?page=3&pageSize=2");

        // Assert
        first!.TotalCount.Should().Be(5);
        first.TotalPages.Should().Be(3);
        first.Items.Select(c => c.Id).Should().Equal(ids[4], ids[3]);
        last!.Items.Select(c => c.Id).Should().Equal(ids[0]);
    }

    [Fact]
    public async Task Browse_chats_falls_back_to_defaults_for_invalid_paging()
    {
        // Act
        var page = await CreateUserClient().GetFromJsonAsync<ChatPageResponse>($"{ChatsUrl}?page=0&pageSize=1000");

        // Assert
        page!.Page.Should().Be(1);
        page.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Browse_chats_without_identity_returns_unauthorized()
    {
        // Act
        var response = await App.CreateClient().GetAsync(ChatsUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
