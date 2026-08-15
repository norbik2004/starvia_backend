using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Emails.Api.Endpoints.Emails;

/// <summary>Centralized route/tag/role constants for the Emails feature endpoints.</summary>
internal static class EmailsEndpoint
{
    public const string BasePath = "v1/emails";
    public const string Tag = "Emails";

    public const string AdminRole = UserRoles.Admin;
}
