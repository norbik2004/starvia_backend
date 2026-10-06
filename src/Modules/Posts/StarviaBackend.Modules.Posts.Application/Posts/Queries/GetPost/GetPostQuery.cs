using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Posts.Application.Posts.Queries.GetPost;

internal sealed record GetPostQuery(string UserId, Guid PostId) : IQuery<PostLongDto>;

/*
 * TODO:
 * Dodac post publications
 * najlepiej liste<PostPublicationDto>
 */
public sealed record PostLongDto(Guid Id, string Title, string Body, PostStatus Status,
    DateTime CreatedAt, string CreatedBy, DateTime? LastModifiedAt);
