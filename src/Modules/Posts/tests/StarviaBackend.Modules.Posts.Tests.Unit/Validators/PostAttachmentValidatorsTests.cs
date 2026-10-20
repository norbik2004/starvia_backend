using FluentValidation.TestHelper;
using StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.AddPostAttachment;
using StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.AddPostAttachments;
using StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.RemovePostAttachment;
using StarviaBackend.Modules.Posts.Application.PostAttachments.Commands.UpdatePostAttachmentOrder;

namespace StarviaBackend.Modules.Posts.Tests.Unit.Validators;

public sealed class AddPostAttachmentsValidatorTests
{
    private readonly AddPostAttachmentsValidator _validator = new();

    private static AddPostAttachmentsCommand Command(
        List<Guid>? fileIds = null,
        Guid? postId = null,
        Guid? userId = null) =>
        new(
            new AddPostAttachmentsRequest(postId ?? Guid.NewGuid(), fileIds ?? [Guid.NewGuid()]),
            userId ?? Guid.NewGuid());

    [Fact]
    public void Valid_command_passes()
    {
        _validator.TestValidate(Command()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_post_id_fails()
    {
        _validator.TestValidate(Command(postId: Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.request.postId)
            .WithErrorMessage("PostId cannot be empty.");
    }

    [Fact]
    public void Empty_user_id_fails()
    {
        _validator.TestValidate(Command(userId: Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.userId)
            .WithErrorMessage("UserId cannot be empty.");
    }

    [Fact]
    public void No_files_fails()
    {
        _validator.TestValidate(Command(fileIds: []))
            .ShouldHaveValidationErrorFor(x => x.request.fileIds)
            .WithErrorMessage("At least one attachment is required.");
    }

    [Fact]
    public void Five_files_pass_and_six_fail()
    {
        var five = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList();
        var six = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToList();

        _validator.TestValidate(Command(fileIds: five)).ShouldNotHaveAnyValidationErrors();
        _validator.TestValidate(Command(fileIds: six))
            .ShouldHaveValidationErrorFor(x => x.request.fileIds)
            .WithErrorMessage("Maximum 5 attachments are allowed per post.");
    }

    [Fact]
    public void Empty_file_id_fails()
    {
        _validator.TestValidate(Command(fileIds: [Guid.NewGuid(), Guid.Empty]))
            .ShouldHaveValidationErrorFor(x => x.request.fileIds)
            .WithErrorMessage("FileId cannot be empty.");
    }
}

public sealed class RemovePostAttachmentValidatorTests
{
    private readonly RemovePostAttachmentValidator _validator = new();

    private static RemovePostAttachmentCommand Command(Guid? postId = null, Guid? attachmentId = null, Guid? userId = null) =>
        new(
            new RemovePostAttachmentRequest(postId ?? Guid.NewGuid(), attachmentId ?? Guid.NewGuid()),
            userId ?? Guid.NewGuid());

    [Fact]
    public void Valid_command_passes()
    {
        _validator.TestValidate(Command()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_post_id_fails()
    {
        _validator.TestValidate(Command(postId: Guid.Empty)).ShouldHaveValidationErrorFor(x => x.request.postId);
    }

    [Fact]
    public void Empty_attachment_id_fails()
    {
        _validator.TestValidate(Command(attachmentId: Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.request.postAttachmentId);
    }

    [Fact]
    public void Empty_user_id_fails()
    {
        _validator.TestValidate(Command(userId: Guid.Empty)).ShouldHaveValidationErrorFor(x => x.userId);
    }
}

public sealed class UpdatePostAttachmentOrderValidatorTests
{
    private readonly UpdatePostAttachmentOrderValidator _validator = new();

    private static UpdatePostAttachmentOrderCommand Command(
        int order = 0,
        Guid? postId = null,
        Guid? attachmentId = null,
        Guid? userId = null) =>
        new(
            new UpdatePostAttachmentOrderRequest(postId ?? Guid.NewGuid(), attachmentId ?? Guid.NewGuid(), order),
            userId ?? Guid.NewGuid());

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void Non_negative_order_passes(int order)
    {
        _validator.TestValidate(Command(order)).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Negative_order_fails()
    {
        _validator.TestValidate(Command(order: -1))
            .ShouldHaveValidationErrorFor(x => x.request.order)
            .WithErrorMessage("Order must be greater than or equal to 0.");
    }

    [Fact]
    public void Empty_ids_fail()
    {
        var result = _validator.TestValidate(Command(postId: Guid.Empty, attachmentId: Guid.Empty, userId: Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.request.postId);
        result.ShouldHaveValidationErrorFor(x => x.request.postAttachmentId);
        result.ShouldHaveValidationErrorFor(x => x.userId);
    }
}
