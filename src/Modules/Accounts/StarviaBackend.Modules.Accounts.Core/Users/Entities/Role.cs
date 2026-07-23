using Microsoft.AspNetCore.Identity;

namespace StarviaBackend.Modules.Accounts.Core.Users.Entities;

internal sealed class Role : IdentityRole<Guid>
{
    private readonly List<UserRole> _userRoles = [];
    private readonly List<RoleClaim> _claims = [];

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
    public IReadOnlyCollection<RoleClaim> Claims => _claims.AsReadOnly();

    public Role()
    {
    }

    public Role(string name) : base(name)
    {
        Id = Guid.NewGuid();
    }
}
