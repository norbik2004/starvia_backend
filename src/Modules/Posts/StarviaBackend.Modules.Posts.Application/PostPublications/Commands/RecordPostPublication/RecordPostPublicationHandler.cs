using StarviaBackend.Modules.Posts.Application.PostPublications.Queries.BrowsePostPublications;
using StarviaBackend.Modules.Posts.Application.Posts.Access;
using StarviaBackend.Modules.Posts.Core.Posts.Access;
using StarviaBackend.Modules.Posts.Core.Posts.Entities;
using StarviaBackend.Modules.Posts.Core.Posts.Enums;
using StarviaBackend.Modules.Posts.Core.Posts.Exceptions;
using StarviaBackend.Modules.Posts.Core.Posts.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Posts.Application.PostPublications.Commands.RecordPostPublication;

internal sealed class RecordPostPublicationHandler(
    IPostRepository postRepository,
    IPostPublicationRepository postPublicationRepository,
    IPostAccessService access)
    : ICommandHandler<RecordPostPublicationCommand, PostPublicationDto>
{
    public async Task<PostPublicationDto> HandleAsync(
        RecordPostPublicationCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = command.Request;

        var post = await postRepository.GetPostByIdAsync(request.PostId, cancellationToken)
            ?? throw new PostNotFoundException(request.PostId);

        access.EnsureAllowed(PostAction.Publish, post);

        var publication = PostPublication.Create(
            request.PostId,
            PostPublicationStatus.Published,
            request.UserPlatformId,
            request.PublishedAt,
            request.ExternalPostId);

        await postPublicationRepository.AddAsync(publication);

        return new PostPublicationDto(
            publication.Id,
            publication.PostId,
            publication.Status,
            publication.UserPlatformId,
            publication.PublishedAt,
            publication.ExternalPostId,
            publication.CreatedAt);
    }
}
