using StarviaBackend.Modules.Posts.Application.PostPublications.Queries.BrowsePostPublications;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Posts.Application.PostPublications.Commands.RecordPostPublication;

public sealed record RecordPostPublicationCommand(RecordPostPublicationRequest Request)
    : ICommand<PostPublicationDto>;

public sealed record RecordPostPublicationRequest(
    Guid PostId,
    Guid UserPlatformId,
    string ExternalPostId,
    DateTime PublishedAt);
