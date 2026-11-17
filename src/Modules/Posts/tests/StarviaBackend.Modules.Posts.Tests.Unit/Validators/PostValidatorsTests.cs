using FluentValidation.TestHelper;
using StarviaBackend.Modules.Posts.Application.Posts.Commands.AddPost;
using StarviaBackend.Modules.Posts.Application.Posts.Commands.RemovePost;
using StarviaBackend.Modules.Posts.Application.Posts.Commands.UpdatePost;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;
using StarviaBackend.Shared.Abstractions.Platforms;
using StarviaBackend.Shared.Abstractions.Posts;

namespace StarviaBackend.Modules.Posts.Tests.Unit.Validators;

public sealed class AddPostValidatorTests
{
    private readonly AddPostValidator _validator = new();

    private static AddPostCommand Command(
        string title,
        IReadOnlyList<PlatformType>? platforms = null) =>
        new(new AddPostRequest(title, platforms ?? [PlatformType.LinkedIn]), Guid.NewGuid());

    [Fact]
    public void Valid_title_passes()
    {
        _validator.TestValidate(Command("A fine title")).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_platforms_fails()
    {
        _validator.TestValidate(Command("A fine title", []))
            .ShouldHaveValidationErrorFor(x => x.request.Platforms);
    }

    [Fact]
    public void Undefined_platform_type_fails()
    {
        _validator.TestValidate(Command("A fine title", [(PlatformType)99]))
            .ShouldHaveValidationErrorFor("request.Platforms[0]");
    }

    [Fact]
    public void Duplicate_platforms_fail()
    {
        _validator.TestValidate(Command("A fine title", [PlatformType.LinkedIn, PlatformType.LinkedIn]))
            .ShouldHaveValidationErrorFor(x => x.request.Platforms);
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
        PostStatus status = PostStatus.Draft,
        IReadOnlyList<PlatformType>? platforms = null) =>
        new(new UpdatePostRequest(Guid.NewGuid(), title, body, status, platforms ?? [PlatformType.LinkedIn]), Guid.NewGuid());

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
    public void Body_at_max_length_passes_and_one_over_fails()
    {
        _validator.TestValidate(Command(body: new string('b', PostLimits.MaxBodyLength))).ShouldNotHaveAnyValidationErrors();
        _validator.TestValidate(Command(body: new string('b', PostLimits.MaxBodyLength + 1))).ShouldHaveValidationErrorFor(x => x.request.Body);
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

    [Fact]
    public void Empty_platforms_fails()
    {
        _validator.TestValidate(Command(platforms: []))
            .ShouldHaveValidationErrorFor(x => x.request.Platforms);
    }

    [Fact]
    public void Undefined_platform_type_fails()
    {
        _validator.TestValidate(Command(platforms: [(PlatformType)99]))
            .ShouldHaveValidationErrorFor("request.Platforms[0]");
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
