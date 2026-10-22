namespace StarviaBackend.Shared.Abstractions.Domain;

/// <summary>
/// Implemented by entities whose audit columns are filled automatically by the
/// EF Core auditing interceptor in Shared.Infrastructure.
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? LastModifiedAt { get; set; }
    string? LastModifiedBy { get; set; }
}
