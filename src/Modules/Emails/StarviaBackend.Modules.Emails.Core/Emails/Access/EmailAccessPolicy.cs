using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Emails.Core.Emails.Access;

/// <summary>
/// Everything a caller can try to do with stored e-mails over the API. Sending is not listed:
/// it only ever happens through the message bus, never on behalf of an API caller. Add a value
/// here, then give it a rule in <see cref="EmailAccessPolicy"/>.
/// </summary>
internal enum EmailAction
{
    /// <summary>
    /// Reading the sent-mail log. Stored bodies contain one-time account links (e-mail confirmation,
    /// password reset), and a stored e-mail has no owner id, so this is admin-only.
    /// </summary>
    BrowseEmails,
}

/// <summary>
/// THE place where access rules for the Emails module are defined. Nothing else in the module
/// looks at roles: handlers and queries ask <c>IEmailAccessService</c>, which asks this policy.
/// </summary>
internal static class EmailAccessPolicy
{
    public static AccessScope ScopeFor(EmailAction action, AccessActor actor) => action switch
    {
        EmailAction.BrowseEmails => actor.AdminOnly(),
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "No access rule defined for this action."),
    };
}