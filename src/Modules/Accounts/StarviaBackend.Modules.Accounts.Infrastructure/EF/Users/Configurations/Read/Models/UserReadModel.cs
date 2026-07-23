namespace StarviaBackend.Modules.Accounts.Infrastructure.EF.Users.Configurations.Read.Models;

/// <summary>Read projection over the accounts Users table (no tracking, query side only).</summary>
internal sealed class UserReadModel
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}
