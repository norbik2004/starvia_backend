using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Accounts.Api.Endpoints.Users;

/// <summary>Centralized route/tag/role constants for the Users feature endpoints.</summary>
internal static class UsersEndpoint
{
    public const string BasePath = "v1/accounts";
    public const string Tag = "Accounts";

    public const string AdminRole = UserRoles.Admin;
}
