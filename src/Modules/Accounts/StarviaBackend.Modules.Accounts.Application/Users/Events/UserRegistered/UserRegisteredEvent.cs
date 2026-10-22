using StarviaBackend.Shared.Abstractions.Messaging;

namespace StarviaBackend.Modules.Accounts.Application.Users.Events.UserRegistered;

/// <summary>
/// Published after an account is created. Other modules/consumers subscribe to react
/// (send a welcome email, provision defaults, ...). Public because it is a bus contract.
/// </summary>
public sealed record UserRegisteredEvent(Guid UserId, string Email, DateTime RegisteredAt) : IIntegrationEvent;
