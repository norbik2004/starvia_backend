namespace StarviaBackend.Shared.Abstractions.Contexts;

/// <summary>The authenticated identity behind the current request (if any).</summary>
public interface IIdentityContext
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    string? Email { get; }
    IReadOnlyList<string> Roles { get; }
}
