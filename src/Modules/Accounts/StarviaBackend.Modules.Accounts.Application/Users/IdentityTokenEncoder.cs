using System.Text;

namespace StarviaBackend.Modules.Accounts.Application.Users;

internal static class IdentityTokenEncoder
{
    public static string Encode(string token)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(token))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static string Decode(string code)
    {
        var padded = code.Replace('-', '+').Replace('_', '/');
        padded += (padded.Length % 4) switch
        {
            2 => "==",
            3 => "=",
            _ => string.Empty
        };

        return Encoding.UTF8.GetString(Convert.FromBase64String(padded));
    }
}
