using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Posts.Application.Posts.Commands.RemovePost;

public sealed record RemovePostCommand(RemovePostRequest request, Guid UserId) : ICommand;

public sealed record RemovePostRequest(Guid postId);
