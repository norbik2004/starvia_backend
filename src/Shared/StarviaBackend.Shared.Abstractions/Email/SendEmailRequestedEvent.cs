using StarviaBackend.Shared.Abstractions.Messaging;

namespace StarviaBackend.Shared.Abstractions.Email;

/// <summary>
/// Queued request to send an outbound email. Published by other modules; consumed by Emails.
/// </summary>
public sealed record SendEmailRequestedEvent(
    Guid UserId,
    string Email,
    string EmailType,
    string? Code = null,
    string? UserName = null,
    string? Link = null) : IIntegrationEvent;
