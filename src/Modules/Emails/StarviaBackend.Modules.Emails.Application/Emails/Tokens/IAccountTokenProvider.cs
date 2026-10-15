namespace StarviaBackend.Modules.Emails.Application.Emails.Tokens;

/// <summary>
/// Source of the Identity tokens embedded in account e-mails. The Emails module does not own user
/// accounts and must not reference the Accounts module, so tokens are obtained through this
/// abstraction (implemented over the message bus in Infrastructure).
/// </summary>
internal interface IAccountTokenProvider
{
    /// <summary>Returns a raw (not yet URL-encoded) e-mail confirmation token for the user.</summary>
    /// <exception cref="Core.Emails.Exceptions.AccountTokenRejectedException">
    /// The account owner refused to issue a token (unknown user, already confirmed, ...).
    /// </exception>
    Task<string> GetEmailConfirmationTokenAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Returns a raw (not yet URL-encoded) password reset token for the user.</summary>
    /// <exception cref="Core.Emails.Exceptions.AccountTokenRejectedException">
    /// The account owner refused to issue a token (unknown user, ...).
    /// </exception>
    Task<string> GetPasswordResetTokenAsync(Guid userId, CancellationToken cancellationToken = default);
}
