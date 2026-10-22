using FluentValidation;

namespace StarviaBackend.Modules.Media.Application.MediaFiles.Commands.UpdateFile;

internal sealed class UpdateFileValidator : AbstractValidator<UpdateFileCommand>
{
    public UpdateFileValidator()
    {
        RuleFor(x => x.request.FileId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.request.Description)
            .MaximumLength(500);
    }
}
