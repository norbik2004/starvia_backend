using Microsoft.AspNetCore.Identity;

namespace StarviaBackend.Modules.Accounts.Core.Users.Entities;

/// <summary>A persisted token (refresh, 2FA, ...) for a <see cref="User"/>.</summary>
internal sealed class UserToken : IdentityUserToken<Guid>;
