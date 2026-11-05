using FluentValidation.TestHelper;
using StarviaBackend.Modules.Ai.Application.PostGeneration.Commands.GeneratePost;

namespace StarviaBackend.Modules.Ai.Tests.Unit.Validators;

public sealed class GeneratePostValidatorTests
{
    private readonly GeneratePostValidator _validator = new();

    private static GeneratePostCommand Command(
        string prompt = "A post about coffee",
        Guid? postId = null,
        Guid? userId = null) =>
        new(new GeneratePostRequest(prompt, postId ?? Guid.NewGuid()), userId ?? Guid.NewGuid());

    [Fact]
    public void Valid_command_passes()
    {
        _validator.TestValidate(Command()).ShouldNotHaveAnyValidationErrors();
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
    public void Missing_prompt_fails()
    {
        _validator.TestValidate(Command(prompt: null!))
            .ShouldHaveValidationErrorFor(x => x.Request.Prompt);
    }

    [Fact]
    public void Prompt_of_max_length_passes()
    {
        _validator.TestValidate(Command(new string('a', 4000))).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Too_long_prompt_fails()
    {
        _validator.TestValidate(Command(new string('a', 4001)))
            .ShouldHaveValidationErrorFor(x => x.Request.Prompt)
            .WithErrorMessage("Prompt cannot exceed 4000 characters.");
    }

    [Fact]
    public void Empty_post_id_fails()
    {
        _validator.TestValidate(Command(postId: Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.Request.PostId)
            .WithErrorMessage("Post ID cannot be empty.");
    }

    [Fact]
    public void Empty_user_id_fails()
    {
        _validator.TestValidate(Command(userId: Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.UserId);
    }
}
