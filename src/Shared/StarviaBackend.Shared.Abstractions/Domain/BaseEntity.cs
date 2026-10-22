namespace StarviaBackend.Shared.Abstractions.Domain;

public abstract class BaseEntity<TId>
{
    public TId Id { get; protected set; } = default!;
}

/// <summary>Convenience base for entities keyed by <see cref="Guid"/>.</summary>
public abstract class BaseEntity : BaseEntity<Guid>;
