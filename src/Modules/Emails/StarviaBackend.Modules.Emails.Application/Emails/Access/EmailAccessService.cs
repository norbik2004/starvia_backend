using StarviaBackend.Modules.Emails.Core.Emails.Access;
using StarviaBackend.Modules.Emails.Core.Emails.Exceptions;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Contexts;

namespace StarviaBackend.Modules.Emails.Application.Emails.Access;

/// <summary>
/// How handlers and queries consult <see cref="EmailAccessPolicy"/> for the caller of the
/// current request. Holds no rules itself.
/// </summary>
internal interface IEmailAccessService
{
    /// <summary>The e-mails the current caller may use for <paramref name="action"/>.</summary>
    AccessScope ScopeFor(EmailAction action);

    /// <summary>
    /// Like <see cref="ScopeFor"/> but throws <see cref="EmailAccessDeniedException"/> when the caller
    /// may not do <paramref name="action"/> at all, so a forbidden listing is not mistaken for an empty one.
    /// </summary>
    AccessScope RequireScopeFor(EmailAction action);
}

internal sealed class EmailAccessService(IContext context) : IEmailAccessService
{
    public AccessScope ScopeFor(EmailAction action) =>
        EmailAccessPolicy.ScopeFor(action, AccessActor.From(context.Identity));

    public AccessScope RequireScopeFor(EmailAction action)
    {
        var scope = ScopeFor(action);
        if (scope.IsEmpty)
        {
            throw new EmailAccessDeniedException(action);
        }

        return scope;
    }
}