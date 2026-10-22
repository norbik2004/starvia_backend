using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace StarviaBackend.Modules.Media.Application.MediaFiles.Commands.RemoveFile;

internal sealed class RemoveFileValidator : AbstractValidator<RemoveFileCommand>
{
    public RemoveFileValidator()
    {
        RuleFor(x => x.FileId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.UserEmail).NotEmpty().EmailAddress();
    }
}
