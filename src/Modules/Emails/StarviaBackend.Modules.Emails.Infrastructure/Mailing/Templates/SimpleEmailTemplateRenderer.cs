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
            EmailType.ConfirmAccountEmail => RenderConfirmAccount(displayName, model.Link),
            EmailType.ResetPasswordEmail => RenderResetPassword(displayName, model.Link),
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

    private static EmailTemplate RenderConfirmAccount(string displayName, string? link)
    {
        var subject = "Confirm your Starvia account";
        var confirmationLink = string.IsNullOrWhiteSpace(link) ? "#" : link;
        var body = Wrap(
            subject,
            $"""
            <p>Hi {Encode(displayName)},</p>
            <p>Please confirm your email address by clicking the button below:</p>
            <p style="margin:28px 0;">
              <a href="{Encode(confirmationLink)}"
                 style="display:inline-block;background:#1a1a1a;color:#ffffff;text-decoration:none;padding:12px 24px;border-radius:6px;font-weight:600;">
                Confirm email
              </a>
            </p>
            <p>If the button does not work, copy and paste this link into your browser:</p>
            <p style="word-break:break-all;font-size:12px;color:#555;">{Encode(confirmationLink)}</p>
            <p>If you did not create an account, you can ignore this message.</p>
            """);

        return new EmailTemplate(subject, body);
    }

    private static EmailTemplate RenderResetPassword(string displayName, string? link)
    {
        var subject = "Reset your Starvia password";
        var resetLink = string.IsNullOrWhiteSpace(link) ? "#" : link;
        var body = Wrap(
            subject,
            $"""
            <p>Hi {Encode(displayName)},</p>
            <p>Please reset your password by clicking the button below:</p>
            <p style="margin:28px 0;">
              <a href="{Encode(resetLink)}"
                 style="display:inline-block;background:#1a1a1a;color:#ffffff;text-decoration:none;padding:12px 24px;border-radius:6px;font-weight:600;">
                Reset password
              </a>
            </p>
            <p>If the button does not work, copy and paste this link into your browser:</p>
            <p style="word-break:break-all;font-size:12px;color:#555;">{Encode(resetLink)}</p>
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
