using StarviaBackend.Shared.Abstractions.Auth;

namespace StarviaBackend.Modules.Calendar.Api.Endpoints;

internal static class ContentCalendarsEndpoint
{
    public const string BasePath = "v1/calendars";
    public const string Tag = "Calendars";

    public const string AdminRole = UserRoles.Admin;
}
