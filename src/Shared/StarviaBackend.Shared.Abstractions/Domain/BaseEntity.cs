namespace StarviaBackend.Shared.Abstractions.Domain;

public abstract class BaseEntity<TId>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public TId Id { get; protected set; } = default!;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>Convenience base for entities keyed by <see cref="Guid"/>.</summary>
public abstract class BaseEntity : BaseEntity<Guid>;
