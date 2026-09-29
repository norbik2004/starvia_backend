using StarviaBackend.Modules.Accounts.Application.Users;

namespace StarviaBackend.Modules.Emails.Application.Emails.Links;

internal static class AccountEmailLink
{
    public static string ConfirmAccount(string frontendBaseUrl, Guid userId, string token)
    {
        var code = IdentityTokenEncoder.Encode(token);
        return $"{Trim(frontendBaseUrl)}/email-confirmed?userId={userId}&code={code}";
    }

    public static string ResetPassword(string frontendBaseUrl, Guid userId, string token)
    {
        var code = IdentityTokenEncoder.Encode(token);
        return $"{Trim(frontendBaseUrl)}/reset-password?userId={userId}&code={code}";
    }

    public static string Login(string frontendBaseUrl) => $"{Trim(frontendBaseUrl)}/login";

    private static string Trim(string frontendBaseUrl) => frontendBaseUrl.TrimEnd('/');
}
