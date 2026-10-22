using Microsoft.AspNetCore.Identity;

namespace StarviaBackend.Modules.Accounts.Core.Users.Entities;

/// <summary>An external login (Google, etc.) linked to a <see cref="User"/>.</summary>
internal sealed class UserLogin : IdentityUserLogin<Guid>;
