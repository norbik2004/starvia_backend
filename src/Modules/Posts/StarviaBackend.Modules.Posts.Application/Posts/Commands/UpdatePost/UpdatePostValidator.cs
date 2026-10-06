using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace StarviaBackend.Modules.Posts.Application.Posts.Commands.UpdatePost;

internal sealed class UpdatePostValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostValidator()
    {
        RuleFor(x => x.request.Title).NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);
        RuleFor(x => x.request.Body).NotEmpty()
            .MinimumLength(0)
            .MaximumLength(1000);
        RuleFor(x => x.request.Status).IsInEnum();
    }
}
