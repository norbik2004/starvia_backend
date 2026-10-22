namespace StarviaBackend.Shared.Abstractions.Messaging;

/// <summary>
/// A contract published on the message bus for other modules/consumers to react to.
/// Keep these immutable and free of domain internals — they are a public contract.
/// </summary>
public interface IIntegrationEvent;
