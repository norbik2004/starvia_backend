using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace StarviaBackend.Modules.Posts.Application.Posts.Commands.RemovePost;

internal sealed class RemovePostValidator : AbstractValidator<RemovePostCommand>
{
    public RemovePostValidator()
    {
        RuleFor(x => x.request.postId).NotEmpty();
    }
}
