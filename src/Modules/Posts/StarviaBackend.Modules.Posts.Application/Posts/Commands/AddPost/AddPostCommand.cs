using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Posts.Application.Posts.Commands.AddPost;

public sealed record AddPostCommand(AddPostRequest request, Guid UserId) : ICommand<AddPostResult>;

public sealed record AddPostRequest(string Title);
public sealed record AddPostResult(Guid PostId);
