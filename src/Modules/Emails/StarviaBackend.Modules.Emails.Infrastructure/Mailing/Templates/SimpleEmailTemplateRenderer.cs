using System.Net;
using StarviaBackend.Modules.Emails.Application.Emails.Templates;
using StarviaBackend.Modules.Emails.Core.Emails.Enums;

namespace StarviaBackend.Modules.Emails.Infrastructure.Mailing.Templates;

internal sealed class SimpleEmailTemplateRenderer : IEmailTemplateRenderer
{
    public EmailTemplate Render(EmailType emailType, EmailTemplateModel model)
    {
        var displayName = string.IsNullOrWhiteSpace(model.UserName)
            ? model.Email
            : model.UserName;

        return emailType switch
        {
            EmailType.WelcomingEmail => RenderWelcome(displayName),
            EmailType.ConfirmAccountEmail => RenderConfirmAccount(displayName, model.Code),
            EmailType.ResetPasswordEmail => RenderResetPassword(displayName, model.Code),
            _ => throw new ArgumentOutOfRangeException(nameof(emailType), emailType, "Unsupported email type."),
        };
    }

    private static EmailTemplate RenderWelcome(string displayName)
    {
        var subject = "Welcome to Starvia";
        var body = Wrap(
            subject,
            $"""
            <p>Hi {Encode(displayName)},</p>
            <p>Welcome aboard — your Starvia account is ready.</p>
            <p>We're glad to have you here.</p>
            """);

        return new EmailTemplate(subject, body);
    }

    private static EmailTemplate RenderConfirmAccount(string displayName, string? code)
    {
        var subject = "Confirm your Starvia account";
        var confirmationCode = string.IsNullOrWhiteSpace(code) ? "—" : code;
        var body = Wrap(
            subject,
            $"""
            <p>Hi {Encode(displayName)},</p>
            <p>Please confirm your email address using the code below:</p>
            <p style="font-size:24px;font-weight:700;letter-spacing:2px;">{Encode(confirmationCode)}</p>
            <p>If you did not create an account, you can ignore this message.</p>
            """);

        return new EmailTemplate(subject, body);
    }

    private static EmailTemplate RenderResetPassword(string displayName, string? code)
    {
        var subject = "Reset your Starvia password";
        var resetCode = string.IsNullOrWhiteSpace(code) ? "—" : code;
        var body = Wrap(
            subject,
            $"""
            <p>Hi {Encode(displayName)},</p>
            <p>Use the code below to reset your password:</p>
            <p style="font-size:24px;font-weight:700;letter-spacing:2px;">{Encode(resetCode)}</p>
            <p>If you did not request a password reset, you can ignore this message.</p>
            """);

        return new EmailTemplate(subject, body);
    }

    private static string Wrap(string title, string content) =>
        $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="utf-8" />
          <title>{Encode(title)}</title>
        </head>
        <body style="margin:0;padding:0;background:#f5f5f5;font-family:Segoe UI,Arial,sans-serif;color:#1a1a1a;">
          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="padding:32px 16px;">
            <tr>
              <td align="center">
                <table role="presentation" width="100%" style="max-width:560px;background:#ffffff;border-radius:8px;padding:32px;">
                  <tr>
                    <td>
                      <p style="margin:0 0 24px;font-size:20px;font-weight:700;">Starvia</p>
                      {content}
                      <p style="margin:32px 0 0;font-size:12px;color:#777;">This message was sent by Starvia.</p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;

    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}
