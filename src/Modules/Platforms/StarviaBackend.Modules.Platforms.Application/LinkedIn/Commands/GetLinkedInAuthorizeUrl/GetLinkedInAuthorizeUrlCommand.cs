using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Platforms.Application.LinkedIn.Commands.GetLinkedInAuthorizeUrl;

public sealed record GetLinkedInAuthorizeUrlCommand(Guid UserId) : ICommand<GetLinkedInAuthorizeUrlResult>;

public sealed record GetLinkedInAuthorizeUrlResult(string Url);
