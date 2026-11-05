using StarviaBackend.Modules.Posts.Application.PostPublications.Queries.BrowsePostPublications;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Posts.Application.PostPublications.Queries.GetPostPublication;

/// <summary>Returns one publication if the caller may view the publications of its post (see the posts access policy).</summary>
internal sealed record GetPostPublicationQuery(Guid PostPublicationId) : IQuery<PostPublicationDto>;
