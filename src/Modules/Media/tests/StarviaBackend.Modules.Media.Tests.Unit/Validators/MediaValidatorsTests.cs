using FluentValidation.TestHelper;
using StarviaBackend.Modules.Media.Application.MediaFiles.Commands.AddFile;
using StarviaBackend.Modules.Media.Application.MediaFiles.Commands.RemoveFile;
using StarviaBackend.Modules.Media.Application.MediaFiles.Commands.UpdateFile;

namespace StarviaBackend.Modules.Media.Tests.Unit.Validators;

public sealed class AddFileValidatorTests
{
    private readonly AddFileValidator _validator = new();

    private static AddFileCommand Command(
        string fileName = "photo.png",
        string contentType = "image/png",
        long length = 1024,
        Stream? content = null,
        string? description = "A photo") =>
        new(new AddFileRequest(fileName, contentType, length, content!, description), Guid.NewGuid(), "user@example.com");

    [Fact]
    public void Valid_command_passes()
    {
        using var stream = new MemoryStream([1, 2, 3]);

        _validator.TestValidate(Command(content: stream)).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_file_name_fails(string fileName)
    {
        using var stream = new MemoryStream();

        _validator.TestValidate(Command(fileName: fileName, content: stream))
            .ShouldHaveValidationErrorFor(x => x.request.FileName);
    }

    [Fact]
    public void File_name_of_100_characters_passes_and_101_fails()
    {
        using var stream = new MemoryStream();

        _validator.TestValidate(Command(fileName: new string('f', 100), content: stream)).ShouldNotHaveAnyValidationErrors();
        _validator.TestValidate(Command(fileName: new string('f', 101), content: stream))
            .ShouldHaveValidationErrorFor(x => x.request.FileName);
    }

    [Fact]
    public void Empty_content_type_fails()
    {
        using var stream = new MemoryStream();

        _validator.TestValidate(Command(contentType: "", content: stream))
            .ShouldHaveValidationErrorFor(x => x.request.ContentType);
    }

    [Fact]
    public void Missing_content_fails()
    {
        _validator.TestValidate(Command(content: null)).ShouldHaveValidationErrorFor(x => x.request.Content);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_positive_length_fails(long length)
    {
        using var stream = new MemoryStream();

        _validator.TestValidate(Command(length: length, content: stream))
            .ShouldHaveValidationErrorFor(x => x.request.Length);
    }

    [Fact]
    public void Length_up_to_50_MB_passes_and_more_fails()
    {
        using var stream = new MemoryStream();

        _validator.TestValidate(Command(length: AddFileValidator.MaxFileSizeInBytes, content: stream))
            .ShouldNotHaveAnyValidationErrors();
        _validator.TestValidate(Command(length: AddFileValidator.MaxFileSizeInBytes + 1, content: stream))
            .ShouldHaveValidationErrorFor(x => x.request.Length);
    }

    [Fact]
    public void Description_of_500_characters_passes_and_501_fails()
    {
        using var stream = new MemoryStream();

        _validator.TestValidate(Command(description: new string('d', 500), content: stream)).ShouldNotHaveAnyValidationErrors();
        _validator.TestValidate(Command(description: new string('d', 501), content: stream))
            .ShouldHaveValidationErrorFor(x => x.request.Description);
    }

    [Fact]
    public void Missing_description_passes()
    {
        using var stream = new MemoryStream();

        _validator.TestValidate(Command(description: null, content: stream)).ShouldNotHaveAnyValidationErrors();
    }
}

public sealed class RemoveFileValidatorTests
{
    private readonly RemoveFileValidator _validator = new();

    private static RemoveFileCommand Command(Guid? fileId = null, Guid? userId = null, string email = "user@example.com") =>
        new(fileId ?? Guid.NewGuid(), userId ?? Guid.NewGuid(), email);

    [Fact]
    public void Valid_command_passes()
    {
        _validator.TestValidate(Command()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_file_id_fails()
    {
        _validator.TestValidate(Command(fileId: Guid.Empty)).ShouldHaveValidationErrorFor(x => x.FileId);
    }

    [Fact]
    public void Empty_user_id_fails()
    {
        _validator.TestValidate(Command(userId: Guid.Empty)).ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Missing_or_invalid_email_fails(string email)
    {
        _validator.TestValidate(Command(email: email)).ShouldHaveValidationErrorFor(x => x.UserEmail);
    }
}

public sealed class UpdateFileValidatorTests
{
    private readonly UpdateFileValidator _validator = new();

    private static UpdateFileCommand Command(Guid? fileId = null, Guid? userId = null, string? description = "Text") =>
        new(new UpdateFileRequest(fileId ?? Guid.NewGuid(), description), userId ?? Guid.NewGuid());

    [Fact]
    public void Valid_command_passes()
    {
        _validator.TestValidate(Command()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Missing_description_passes()
    {
        _validator.TestValidate(Command(description: null)).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_file_id_fails()
    {
        _validator.TestValidate(Command(fileId: Guid.Empty)).ShouldHaveValidationErrorFor(x => x.request.FileId);
    }

    [Fact]
    public void Empty_user_id_fails()
    {
        _validator.TestValidate(Command(userId: Guid.Empty)).ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Description_longer_than_500_characters_fails()
    {
        _validator.TestValidate(Command(description: new string('d', 500))).ShouldNotHaveAnyValidationErrors();
        _validator.TestValidate(Command(description: new string('d', 501)))
            .ShouldHaveValidationErrorFor(x => x.request.Description);
    }
}
