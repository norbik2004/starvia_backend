namespace StarviaBackend.Shared.Abstractions.Accounts;

/// <summary>
/// Bus request (MassTransit request/response) asking the Accounts module for an e-mail confirmation
/// token for <paramref name="UserId"/>. Answered with <see cref="UserTokenGenerated"/> or
/// <see cref="UserTokenRejected"/>.
/// </summary>
public sealed record GenerateEmailConfirmationTokenRequest(Guid UserId);

/// <summary>
/// Bus request asking the Accounts module for a password reset token for <paramref name="UserId"/>.
/// Answered with <see cref="UserTokenGenerated"/> or <see cref="UserTokenRejected"/>.
/// </summary>
public sealed record GeneratePasswordResetTokenRequest(Guid UserId);

/// <summary>Successful response: the raw (not yet URL-encoded) Identity token.</summary>
public sealed record UserTokenGenerated(string Token);

/// <summary>
/// Expected business failure (e.g. <c>user_not_found</c>, <c>email_arleady_confirmed</c>).
/// Lets the requester tell "this can never succeed" apart from a transient bus failure.
/// </summary>
public sealed record UserTokenRejected(string Code, string Reason);
