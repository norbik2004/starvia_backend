using FluentValidation.TestHelper;
using StarviaBackend.Modules.Posts.Application.Posts.Commands.AddPost;
using StarviaBackend.Modules.Posts.Application.Posts.Commands.RemovePost;
using StarviaBackend.Modules.Posts.Application.Posts.Commands.UpdatePost;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;

namespace StarviaBackend.Modules.Posts.Tests.Unit.Validators;

public sealed class AddPostValidatorTests
{
    private readonly AddPostValidator _validator = new();

    private static AddPostCommand Command(string title) => new(new AddPostRequest(title), Guid.NewGuid());

    [Fact]
    public void Valid_title_passes()
    {
        _validator.TestValidate(Command("A fine title")).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ab")]
    public void Empty_or_too_short_title_fails(string title)
    {
        _validator.TestValidate(Command(title)).ShouldHaveValidationErrorFor(x => x.request.Title);
    }

    [Fact]
    public void Title_of_100_characters_passes_and_101_fails()
    {
        _validator.TestValidate(Command(new string('a', 100))).ShouldNotHaveAnyValidationErrors();
        _validator.TestValidate(Command(new string('a', 101))).ShouldHaveValidationErrorFor(x => x.request.Title);
    }
}

public sealed class UpdatePostValidatorTests
{
    private readonly UpdatePostValidator _validator = new();

    private static UpdatePostCommand Command(
        string title = "A fine title",
        string body = "Body",
        PostStatus status = PostStatus.Draft) =>
        new(new UpdatePostRequest(Guid.NewGuid(), title, body, status), Guid.NewGuid());

    [Fact]
    public void Valid_request_passes()
    {
        _validator.TestValidate(Command()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void Invalid_title_fails(string title)
    {
        _validator.TestValidate(Command(title: title)).ShouldHaveValidationErrorFor(x => x.request.Title);
    }

    [Fact]
    public void Body_longer_than_1000_characters_fails()
    {
        _validator.TestValidate(Command(body: new string('b', 1000))).ShouldNotHaveAnyValidationErrors();
        _validator.TestValidate(Command(body: new string('b', 1001))).ShouldHaveValidationErrorFor(x => x.request.Body);
    }

    [Fact]
    public void Empty_body_passes()
    {
        _validator.TestValidate(Command(body: string.Empty)).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Undefined_status_fails()
    {
        _validator.TestValidate(Command(status: (PostStatus)99)).ShouldHaveValidationErrorFor(x => x.request.Status);
    }
}

public sealed class RemovePostValidatorTests
{
    private readonly RemovePostValidator _validator = new();

    [Fact]
    public void Existing_post_id_passes()
    {
        var command = new RemovePostCommand(new RemovePostRequest(Guid.NewGuid()), Guid.NewGuid());

        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_post_id_fails()
    {
        var command = new RemovePostCommand(new RemovePostRequest(Guid.Empty), Guid.NewGuid());

        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.request.postId);
    }
}
