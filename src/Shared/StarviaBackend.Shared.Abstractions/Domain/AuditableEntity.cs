namespace StarviaBackend.Shared.Abstractions.Domain;

/// <summary>A <see cref="BaseEntity{TId}"/> whose audit columns are maintained automatically.</summary>
public abstract class AuditableEntity<TId> : BaseEntity<TId>, IAuditable
{
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

public abstract class AuditableEntity : AuditableEntity<Guid>;
