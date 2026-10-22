using Microsoft.AspNetCore.Identity;

namespace StarviaBackend.Modules.Accounts.Core.Users.Entities;

/// <summary>A claim attached to a <see cref="Role"/>, inherited by its members.</summary>
internal sealed class RoleClaim : IdentityRoleClaim<Guid>;
