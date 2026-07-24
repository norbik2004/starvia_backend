using Microsoft.AspNetCore.Identity;

namespace StarviaBackend.Modules.Accounts.Core.Users.Entities;

/// <summary>
/// The account aggregate. Built on ASP.NET Identity but with a private constructor and factory
/// methods so creation/mutation go through the domain. Exposes its roles, claims and logins as
/// read-only navigations for the query side.
/// </summary>
internal sealed class User : IdentityUser<Guid>
{
    private readonly List<UserRole> _roles = [];
    private readonly List<UserClaim> _claims = [];
    private readonly List<UserLogin> _logins = [];

    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();
    public IReadOnlyCollection<UserClaim> Claims => _claims.AsReadOnly();
    public IReadOnlyCollection<UserLogin> Logins => _logins.AsReadOnly();

    // EF / Identity materialization.
    private User()
    {
    }

    public static User Create(string email, DateTime createdAt)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email.Split("@").First(),
            CreatedAt = createdAt,
        };
    }

    public void ChangeUserName(string userName) => UserName = userName;

    public void RecordLogin(DateTime at) => LastLoginAt = at;
}
