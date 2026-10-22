using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Posts.Application.Posts.Commands.UpdatePost;

public sealed record UpdatePostCommand(UpdatePostRequest request, Guid UserId) : ICommand<UpdatePostResult>;

public sealed record UpdatePostRequest(Guid PostId, string Title, string Body, PostStatus Status);
public sealed record UpdatePostResult(Guid PostId);
