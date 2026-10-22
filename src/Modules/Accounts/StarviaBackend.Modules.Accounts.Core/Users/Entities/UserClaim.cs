using Microsoft.AspNetCore.Identity;

namespace StarviaBackend.Modules.Accounts.Core.Users.Entities;

/// <summary>A claim granted directly to a <see cref="User"/> (permissions, tenant, etc.).</summary>
internal sealed class UserClaim : IdentityUserClaim<Guid>;
