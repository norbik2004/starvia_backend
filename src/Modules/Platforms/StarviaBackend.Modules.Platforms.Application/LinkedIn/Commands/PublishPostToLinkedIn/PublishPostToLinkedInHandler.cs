using StarviaBackend.Modules.Media.Integrations;
using StarviaBackend.Modules.Platforms.Application.Access;
using StarviaBackend.Modules.Platforms.Core.Platforms.Access;
using StarviaBackend.Modules.Platforms.Core.Platforms.Exceptions;
using StarviaBackend.Modules.Platforms.Core.Platforms.Repositories;
using StarviaBackend.Modules.Platforms.Integrations.LinkedIn;
using StarviaBackend.Modules.Posts.Integrations;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Platforms.Application.LinkedIn.Commands.PublishPostToLinkedIn;

internal sealed class PublishPostToLinkedInHandler(
    IUserPlatformRepository userPlatformRepository,
    IPlatformRepository platformRepository,
    IPlatformAccessService access,
    IPostsClient postsClient,
    IMediaClient mediaClient,
    ILinkedInClient linkedInClient)
    : ICommandHandler<PublishPostToLinkedInCommand, PublishPostToLinkedInResult>
{
    public async Task<PublishPostToLinkedInResult> HandleAsync(
        PublishPostToLinkedInCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = command.Request;

        var userPlatform = await userPlatformRepository.GetByIdAsync(request.UserPlatformId, cancellationToken)
            ?? throw new UserPlatformNotFoundException(request.UserPlatformId);

        access.EnsureAllowed(PlatformAction.ViewUserPlatform, userPlatform.Id, userPlatform.UserId.ToString());

        var platform = await platformRepository.GetByIdAsync(userPlatform.PlatformId, cancellationToken)
            ?? throw new PlatformNotFoundException(userPlatform.PlatformId);

        if (platform.PlatformType != PlatformType.LinkedIn)
        {
            throw new UserPlatformPlatformMismatchException(request.UserPlatformId, nameof(PlatformType.LinkedIn));
        }

        if (string.IsNullOrWhiteSpace(userPlatform.AccessToken)
            || string.IsNullOrWhiteSpace(userPlatform.ExternalAccountId))
        {
            throw new UserPlatformNotConnectedException(request.UserPlatformId);
        }

        var post = await postsClient.GetPostForPublishAsync(command.UserId, request.PostId, cancellationToken);

        if (string.IsNullOrWhiteSpace(post.Body))
        {
            throw new PostBodyRequiredException();
        }

        string externalPostId;

        if (post.Attachments.Count > 0)
        {
            var fileIds = post.Attachments.OrderBy(a => a.Order).Select(a => a.FileId);
            // Ownership check against the media API (auth-forwarding HttpClient).
            var files = await mediaClient.GetFilesAsync(command.UserId, fileIds, cancellationToken);
            var images = await DownloadImagesAsync(files, cancellationToken);

            try
            {
                externalPostId = await linkedInClient.PublishImagePostAsync(
                    new LinkedInImagePost(
                        userPlatform.AccessToken,
                        userPlatform.ExternalAccountId,
                        post.Body,
                        images.Select(i => i.Source).ToList()),
                    cancellationToken);
            }
            finally
            {
                foreach (var image in images)
                {
                    await image.Source.Content.DisposeAsync();
                }
            }
        }
        else if (!string.IsNullOrWhiteSpace(request.ArticleUrl))
        {
            externalPostId = await linkedInClient.PublishArticlePostAsync(
                new LinkedInArticlePost(
                    userPlatform.AccessToken,
                    userPlatform.ExternalAccountId,
                    post.Body,
                    request.ArticleUrl,
                    string.IsNullOrWhiteSpace(post.Title) ? null : post.Title,
                    request.ArticleDescription),
                cancellationToken);
        }
        else
        {
            externalPostId = await linkedInClient.PublishTextPostAsync(
                new LinkedInTextPost(userPlatform.AccessToken, userPlatform.ExternalAccountId, post.Body),
                cancellationToken);
        }

        var publishedAt = DateTime.UtcNow;
        var publication = await postsClient.CreatePublicationAsync(
            command.UserId,
            new CreatePostPublicationRequest(
                request.PostId,
                request.UserPlatformId,
                externalPostId,
                publishedAt),
            cancellationToken);

        return new PublishPostToLinkedInResult(publication.Id, publication.ExternalPostId, publication.PublishedAt);
    }

    private async Task<List<DownloadedImage>> DownloadImagesAsync(
        IReadOnlyList<MediaFile> files,
        CancellationToken cancellationToken)
    {
        var images = new List<DownloadedImage>(files.Count);

        foreach (var file in files)
        {
            // Must go through IMediaClient so the caller's bearer token is forwarded —
            // GET /v1/media-files/{id}/content is [Authorize] and rejects anonymous downloads with 401.
            var content = await mediaClient.DownloadContentAsync(file.Id, cancellationToken);
            images.Add(new DownloadedImage(new LinkedInImageSource(content.Content, content.ContentType)));
        }

        return images;
    }

    private sealed record DownloadedImage(LinkedInImageSource Source);
}
