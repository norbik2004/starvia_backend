using StarviaBackend.Shared.Abstractions.Contexts;

namespace StarviaBackend.Shared.Abstractions.Auth;

/// <summary>Who is asking. <see cref="UserId"/> is null for anonymous callers.</summary>
public sealed record AccessActor(Guid? UserId, bool IsAdmin)
{
    public static AccessActor Anonymous => new(null, false);

    public static AccessActor From(IIdentityContext identity) =>
        new(identity.UserId, identity.Roles.Contains(UserRoles.Admin));

    /// <summary>Admins get <see cref="AccessScope.All"/>, everybody else only the resources they created.</summary>
    public AccessScope OwnerOrAdmin()
    {
        if (IsAdmin)
        {
            return AccessScope.All;
        }

        return UserId is { } userId ? AccessScope.OwnedBy(userId) : AccessScope.None;
    }

    /// <summary>Only the user who created the resource, admins included: for private data an admin must not see or change.</summary>
    public AccessScope OwnerOnly() => UserId is { } userId ? AccessScope.OwnedBy(userId) : AccessScope.None;

    /// <summary>Only admins.</summary>
    public AccessScope AdminOnly() => IsAdmin ? AccessScope.All : AccessScope.None;

    /// <summary>Any authenticated user, regardless of who created the resource.</summary>
    public AccessScope Authenticated() => UserId is not null || IsAdmin ? AccessScope.All : AccessScope.None;
}

/// <summary>
/// The set of resources an actor may use for a given action. The same value answers
/// "may I touch this one resource?" (<see cref="Allows"/>) and "which resources may I list?"
/// (<see cref="Unrestricted"/> / <see cref="OwnerId"/>, applied as a query filter), so single-resource
/// checks and listings can never disagree. Each module owns a policy that maps its actions to scopes.
/// </summary>
public readonly record struct AccessScope(bool Unrestricted, string? OwnerId)
{
    public static AccessScope All => new(true, null);
    public static AccessScope None => new(false, null);
    public static AccessScope OwnedBy(Guid userId) => new(false, userId.ToString());

    /// <summary>True when nothing can match, so a listing can short-circuit to an empty result.</summary>
    public bool IsEmpty => !Unrestricted && OwnerId is null;

    public bool Allows(string? resourceOwnerId) =>
        Unrestricted || (OwnerId is not null && OwnerId == resourceOwnerId);
}
