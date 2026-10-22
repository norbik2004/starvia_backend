using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace StarviaBackend.Modules.Posts.Application.Posts.Commands.AddPost;

internal sealed class AddPostValidator : AbstractValidator<AddPostCommand>
{
    public AddPostValidator()
    {
        RuleFor(x => x.request.Title).NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);
    }
}
