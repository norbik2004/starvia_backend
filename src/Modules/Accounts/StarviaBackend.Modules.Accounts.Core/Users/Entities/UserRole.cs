using Microsoft.AspNetCore.Identity;

namespace StarviaBackend.Modules.Accounts.Core.Users.Entities;

/// <summary>Join between a <see cref="User"/> and a <see cref="Role"/>.</summary>
internal sealed class UserRole : IdentityUserRole<Guid>
{
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;
}
