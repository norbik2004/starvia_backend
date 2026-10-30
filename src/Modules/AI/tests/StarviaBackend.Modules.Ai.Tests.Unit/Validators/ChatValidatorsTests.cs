using FluentValidation.TestHelper;
using StarviaBackend.Modules.Ai.Application.Chats.Commands.AddChat;
using StarviaBackend.Modules.Ai.Application.Chats.Commands.AddChatMessage;
using StarviaBackend.Modules.Ai.Application.Chats.Commands.RemoveChat;
using StarviaBackend.Modules.Ai.Application.Chats.Commands.UpdateChat;
using StarviaBackend.Modules.Ai.Application.Chats.Queries.BrowseChats;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;

namespace StarviaBackend.Modules.Ai.Tests.Unit.Validators;

public sealed class AddChatValidatorTests
{
    private readonly AddChatValidator _validator = new();

    private static AddChatCommand Command(
        string title = "Ideas",
        ChatType type = ChatType.AskAgent,
        Guid? userId = null) =>
        new(new AddChatRequest(type, title), userId ?? Guid.NewGuid());

    [Theory]
    [InlineData(ChatType.AskAgent)]
    [InlineData(ChatType.PostContentGeneration)]
    public void Valid_command_passes(ChatType type)
    {
        _validator.TestValidate(Command(type: type)).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_title_fails(string title)
    {
        _validator.TestValidate(Command(title))
            .ShouldHaveValidationErrorFor(x => x.Request.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void Title_of_max_length_passes()
    {
        _validator.TestValidate(Command(new string('a', 150))).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Too_long_title_fails()
    {
        _validator.TestValidate(Command(new string('a', 151)))
            .ShouldHaveValidationErrorFor(x => x.Request.Title)
            .WithErrorMessage("Title cannot exceed 150 characters.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    public void Unknown_type_fails(int type)
    {
        _validator.TestValidate(Command(type: (ChatType)type))
            .ShouldHaveValidationErrorFor(x => x.Request.Type)
            .WithErrorMessage("Chat type is not supported.");
    }

    [Fact]
    public void Empty_user_fails()
    {
        _validator.TestValidate(Command(userId: Guid.Empty)).ShouldHaveValidationErrorFor(x => x.UserId);
    }
}

public sealed class UpdateChatValidatorTests
{
    private readonly UpdateChatValidator _validator = new();

    private static UpdateChatCommand Command(
        string title = "Ideas",
        ChatStatus status = ChatStatus.Active,
        Guid? chatId = null) =>
        new(chatId ?? Guid.NewGuid(), new UpdateChatRequest(title, status), Guid.NewGuid());

    [Theory]
    [InlineData(ChatStatus.Active)]
    [InlineData(ChatStatus.Archived)]
    public void Valid_command_passes(ChatStatus status)
    {
        _validator.TestValidate(Command(status: status)).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_chat_id_fails()
    {
        _validator.TestValidate(Command(chatId: Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.ChatId)
            .WithErrorMessage("Chat ID is required.");
    }

    [Fact]
    public void Blank_title_fails()
    {
        _validator.TestValidate(Command(title: " "))
            .ShouldHaveValidationErrorFor(x => x.Request.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void Too_long_title_fails()
    {
        _validator.TestValidate(Command(title: new string('a', 151)))
            .ShouldHaveValidationErrorFor(x => x.Request.Title)
            .WithErrorMessage("Title cannot exceed 150 characters.");
    }

    [Fact]
    public void Unknown_status_fails()
    {
        _validator.TestValidate(Command(status: (ChatStatus)99))
            .ShouldHaveValidationErrorFor(x => x.Request.Status)
            .WithErrorMessage("Chat status is not supported.");
    }
}

public sealed class RemoveChatValidatorTests
{
    private readonly RemoveChatValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        _validator.TestValidate(new RemoveChatCommand(Guid.NewGuid(), Guid.NewGuid()))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_chat_id_fails()
    {
        _validator.TestValidate(new RemoveChatCommand(Guid.Empty, Guid.NewGuid()))
            .ShouldHaveValidationErrorFor(x => x.ChatId)
            .WithErrorMessage("Chat ID is required.");
    }

    [Fact]
    public void Empty_user_fails()
    {
        _validator.TestValidate(new RemoveChatCommand(Guid.NewGuid(), Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.UserId);
    }
}

public sealed class AddChatMessageValidatorTests
{
    private readonly AddChatMessageValidator _validator = new();

    private static AddChatMessageCommand Command(string content = "Hello", Guid? chatId = null) =>
        new(chatId ?? Guid.NewGuid(), new AddChatMessageRequest(content), Guid.NewGuid());

    [Fact]
    public void Valid_command_passes()
    {
        _validator.TestValidate(Command()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_chat_id_fails()
    {
        _validator.TestValidate(Command(chatId: Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.ChatId)
            .WithErrorMessage("Chat ID is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_content_fails(string content)
    {
        _validator.TestValidate(Command(content))
            .ShouldHaveValidationErrorFor(x => x.Request.Content)
            .WithErrorMessage("Content is required.");
    }

    [Fact]
    public void Content_of_max_length_passes()
    {
        _validator.TestValidate(Command(new string('a', 8000))).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Too_long_content_fails()
    {
        _validator.TestValidate(Command(new string('a', 8001)))
            .ShouldHaveValidationErrorFor(x => x.Request.Content)
            .WithErrorMessage("Content cannot exceed 8000 characters.");
    }
}

public sealed class BrowseChatsQueryTests
{
    [Fact]
    public void Defaults_start_on_the_first_page_of_twenty()
    {
        var query = new BrowseChatsQuery(Guid.NewGuid());

        Assert.Equal(1, query.Page);
        Assert.Equal(20, query.PageSize);
        Assert.Null(query.Type);
        Assert.Null(query.Status);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(1, 1)]
    [InlineData(7, 7)]
    public void Page_below_one_falls_back_to_the_first_page(int page, int expected)
    {
        var query = new BrowseChatsQuery(Guid.NewGuid(), Page: page);

        Assert.Equal(expected, query.Page);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(101, 20)]
    [InlineData(1, 1)]
    [InlineData(100, 100)]
    public void Page_size_outside_the_allowed_range_falls_back_to_the_default(int pageSize, int expected)
    {
        var query = new BrowseChatsQuery(Guid.NewGuid(), PageSize: pageSize);

        Assert.Equal(expected, query.PageSize);
    }
}
