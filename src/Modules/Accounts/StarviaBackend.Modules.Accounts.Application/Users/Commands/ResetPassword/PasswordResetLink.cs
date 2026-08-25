namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.ResetPassword;

internal static class PasswordResetLink
{
    public const string FrontendRelativePath = "reset-password";

    public static string Build(string frontendBaseUrl, Guid userId, string token)
    {
        var code = IdentityTokenEncoder.Encode(token);
        return $"{frontendBaseUrl.TrimEnd('/')}/{FrontendRelativePath}?userId={userId}&code={code}";
    }

    public static string EncodeToken(string token) => IdentityTokenEncoder.Encode(token);
}
