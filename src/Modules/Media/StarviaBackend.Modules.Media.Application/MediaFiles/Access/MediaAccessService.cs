using StarviaBackend.Modules.Media.Core.Media.Access;
using StarviaBackend.Modules.Media.Core.Media.Entities;
using StarviaBackend.Modules.Media.Core.Media.Exceptions;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Contexts;

namespace StarviaBackend.Modules.Media.Application.MediaFiles.Access;

/// <summary>
/// How handlers and queries consult <see cref="MediaAccessPolicy"/> for the caller of the
/// current request. Holds no rules itself.
/// </summary>
internal interface IMediaAccessService
{
    /// <summary>The files the current caller may use for <paramref name="action"/>; apply it as a filter when listing.</summary>
    AccessScope ScopeFor(MediaAction action);

    /// <summary>Throws <see cref="MediaAccessDeniedException"/> unless the caller may do <paramref name="action"/> on the file.</summary>
    void EnsureAllowed(MediaAction action, Guid fileId, string? fileOwnerId);

    void EnsureAllowed(MediaAction action, MediaFile file) => EnsureAllowed(action, file.Id, file.CreatedBy);
}

internal sealed class MediaAccessService(IContext context) : IMediaAccessService
{
    public AccessScope ScopeFor(MediaAction action) =>
        MediaAccessPolicy.ScopeFor(action, AccessActor.From(context.Identity));

    public void EnsureAllowed(MediaAction action, Guid fileId, string? fileOwnerId)
    {
        if (!ScopeFor(action).Allows(fileOwnerId))
        {
            throw new MediaAccessDeniedException(fileId, action);
        }
    }
}
