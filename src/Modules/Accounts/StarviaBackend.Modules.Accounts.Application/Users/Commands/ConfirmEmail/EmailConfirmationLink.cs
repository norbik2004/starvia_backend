using System.Security.Cryptography;
using System.Text;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.ConfirmEmail;

internal static class EmailConfirmationLink
{
    public const string ApiRelativePath = "v1/accounts/confirm-email";
    public const string FrontendRelativePath = "email-confirmed";
    public const int TicketLength = 8;

    private const string TicketAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string Build(string apiBaseUrl, Guid userId, string token)
    {
        var code = Encode(token);
        return $"{apiBaseUrl.TrimEnd('/')}/{ApiRelativePath}?userId={userId}&code={code}";
    }

    public static string FrontendRedirect(string frontendBaseUrl, string? errorCode = null)
    {
        var status = string.IsNullOrWhiteSpace(errorCode) ? "success" : "error";
        var ticket = NewTicket();
        var url = $"{frontendBaseUrl.TrimEnd('/')}/{FrontendRelativePath}?status={status}&ticket={ticket}";

        return string.IsNullOrWhiteSpace(errorCode)
            ? url
            : $"{url}&error={Uri.EscapeDataString(errorCode)}";
    }

    public static string NewTicket()
    {
        Span<char> chars = stackalloc char[TicketLength];
        for (var i = 0; i < TicketLength; i++)
        {
            chars[i] = TicketAlphabet[RandomNumberGenerator.GetInt32(TicketAlphabet.Length)];
        }

        return new string(chars);
    }

    public static string DecodeToken(string code)
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

    private static string Encode(string token)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(token))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
