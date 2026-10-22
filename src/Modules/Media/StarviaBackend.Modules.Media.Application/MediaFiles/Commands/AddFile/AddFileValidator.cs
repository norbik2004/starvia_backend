using FluentValidation;

namespace StarviaBackend.Modules.Media.Application.MediaFiles.Commands.AddFile;

internal sealed class AddFileValidator : AbstractValidator<AddFileCommand>
{
    public const long MaxFileSizeInBytes = 50 * 1024 * 1024;

    public AddFileValidator()
    {
        RuleFor(x => x.request.FileName).NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.request.ContentType).NotEmpty();

        RuleFor(x => x.request.Content).NotNull();

        RuleFor(x => x.request.Length)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxFileSizeInBytes);

        RuleFor(x => x.request.Description)
            .MaximumLength(500);
    }
}
