using FluentValidation.TestHelper;
using StarviaBackend.Modules.Ai.Application.AskAgent.Commands.AskAgent;
using StarviaBackend.Modules.Ai.Core.Chats.Entities;

namespace StarviaBackend.Modules.Ai.Tests.Unit.Validators;

public sealed class AskAgentValidatorTests
{
    private readonly AskAgentValidator _validator = new();

    private static AskAgentCommand Command(
        string prompt = "How should I open a LinkedIn post?",
        Guid? chatId = null,
        string? title = null,
        Guid? brandProfileId = null,
        Guid? userId = null) =>
        new(new AskAgentRequest(prompt, chatId, title, brandProfileId), userId ?? Guid.NewGuid());

    [Fact]
    public void Valid_command_passes()
    {
        _validator.TestValidate(Command()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Valid_command_with_optional_fields_passes()
    {
        _validator.TestValidate(Command(
                chatId: Guid.NewGuid(),
                title: "Strategie",
                brandProfileId: Guid.NewGuid()))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_prompt_fails(string prompt)
    {
        _validator.TestValidate(Command(prompt))
            .ShouldHaveValidationErrorFor(x => x.Request.Prompt)
            .WithErrorMessage("Prompt is required.");
    }

    [Fact]
    public void Prompt_of_max_length_passes()
    {
        _validator.TestValidate(Command(new string('a', AskAgentRequest.MaxPromptLength)))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Too_long_prompt_fails()
    {
        _validator.TestValidate(Command(new string('a', AskAgentRequest.MaxPromptLength + 1)))
            .ShouldHaveValidationErrorFor(x => x.Request.Prompt)
            .WithErrorMessage($"Prompt cannot exceed {AskAgentRequest.MaxPromptLength} characters.");
    }

    [Fact]
    public void Empty_chat_id_fails()
    {
        _validator.TestValidate(Command(chatId: Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.Request.ChatId)
            .WithErrorMessage("Chat ID cannot be empty.");
    }

    [Fact]
    public void Empty_brand_profile_id_fails()
    {
        _validator.TestValidate(Command(brandProfileId: Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.Request.BrandProfileId)
            .WithErrorMessage("Brand profile ID cannot be empty.");
    }

    [Fact]
    public void Too_long_title_fails()
    {
        _validator.TestValidate(Command(title: new string('t', Chat.MaxTitleLength + 1)))
            .ShouldHaveValidationErrorFor(x => x.Request.Title)
            .WithErrorMessage($"Title cannot exceed {Chat.MaxTitleLength} characters.");
    }

    [Fact]
    public void Empty_user_id_fails()
    {
        _validator.TestValidate(Command(userId: Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.UserId);
    }
}
