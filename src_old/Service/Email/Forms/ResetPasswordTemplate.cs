using Core.Application.DTO.Email.Models;
using Microsoft.AspNetCore.Identity.Data;
using System;

namespace Service.Email.Forms
{
    public static class ResetPasswordTemplate
    {
        public static string GenerateUserMessageTemplate(
            PasswordResetEmailRequest request,
            string frontendURL)
        {
            var url =
                $"{frontendURL}/reset-password?userId={request.UserId}&token={Uri.EscapeDataString(request.Token)}";

            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1'>
  <meta name='x-apple-disable-message-reformatting'>
  <title>Reset your password</title>
</head>

<body style='margin:0;padding:0;background-color:#ffffff;'>
  <!-- Preheader (hidden) -->
  <div style='display:none;max-height:0;overflow:hidden;opacity:0;color:transparent;mso-hide:all;'>
    Reset your Starvia password securely.
  </div>

  <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='width:100%;border-collapse:collapse;background-color:#ffffff;'>
    <tr>
      <td align='center' style='padding:40px 16px;background:#ffffff;'>
        <table role='presentation' width='640' cellspacing='0' cellpadding='0' border='0' style='width:640px;max-width:100%;border-collapse:separate;border-spacing:0;'>
          <tr>
            <td style='padding:0;'>
              <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='width:100%;border-collapse:separate;border-spacing:0;border-radius:16px;overflow:hidden;background:#ffffff;'>
                <tr>
                  <td style='padding:0;background:linear-gradient(180deg,#ffffff 0%, #ffffff 38%, #eaf3ff 55%, #cfe5ff 75%, #b7d8ff 100%);'>
                    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='width:100%;border-collapse:collapse;'>
                      <tr>
                        <td style='padding:28px 24px 14px 24px;text-align:left;'>
                          <div style='font-family:Inter,system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;font-size:12px;letter-spacing:1.2px;text-transform:uppercase;color:#0b63ce;font-weight:700;'>
                            Starvia
                          </div>
                        </td>
                      </tr>

                      <tr>
                        <td style='padding:0 24px 24px 24px;'>
                          <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0'
                                 style='width:100%;border-collapse:separate;border-spacing:0;background:#ffffff;border:1px solid #dbe7f7;border-radius:16px;overflow:hidden;box-shadow:0 18px 48px rgba(15,23,42,0.10);'>
                            <tr>
                              <td style='padding:26px 22px 18px 22px;text-align:left;background:linear-gradient(135deg,#0b63ce 0%, #1f86ff 55%, #2b5cff 100%);'>
                                <div style='font-family:Inter,system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;font-size:22px;line-height:1.25;color:#ffffff;font-weight:800;'>
                                  Reset your password
                                </div>
                                <div style='margin-top:6px;font-family:Inter,system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;font-size:14px;line-height:1.5;color:rgba(255,255,255,0.88);'>
                                  Choose a new password to regain access to your account.
                                </div>
                              </td>
                            </tr>

                            <tr>
                              <td style='padding:22px 22px 10px 22px;text-align:left;'>
                                <div style='font-family:Inter,system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;font-size:14px;line-height:1.7;color:#475569;'>
                                  We received a request to reset your password. Click the button below to set a new one.
                                </div>

                                <table role='presentation' cellspacing='0' cellpadding='0' border='0' style='margin-top:18px;border-collapse:separate;'>
                                  <tr>
                                    <td align='center' style='border-radius:12px;background:linear-gradient(135deg,#0b63ce 0%, #1f86ff 55%, #2b5cff 100%);'>
                                      <a href='{url}'
                                         style='display:inline-block;padding:13px 18px;font-family:Inter,system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;font-size:14px;line-height:1.2;font-weight:800;color:#ffffff;text-decoration:none;border-radius:12px;'>
                                        Reset Password
                                      </a>
                                    </td>
                                  </tr>
                                </table>

                                <div style='margin-top:14px;font-family:Inter,system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;font-size:12px;line-height:1.6;color:#64748b;'>
                                  For your security, this link may expire. If it does, request another reset email.
                                </div>

                                <div style='margin-top:16px;padding:12px 12px;border-radius:12px;background:#f6f9ff;border:1px solid #e3edff;'>
                                  <div style='font-family:Inter,system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;font-size:12px;line-height:1.6;color:#475569;'>
                                    If the button doesn’t work, copy and paste this link into your browser:
                                  </div>
                                  <div style='margin-top:6px;font-family:Consolas,ui-monospace,SFMono-Regular,Menlo,Monaco,monospace;font-size:12px;line-height:1.55;color:#0b63ce;word-break:break-all;'>
                                    <a href='{url}' style='color:#0b63ce;text-decoration:underline;'> {url} </a>
                                  </div>
                                </div>

                                <div style='margin-top:14px;font-family:Inter,system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;font-size:12px;line-height:1.6;color:#64748b;'>
                                  If you didn’t request a password reset, you can ignore this email. Your password will not change.
                                </div>
                              </td>
                            </tr>

                            <tr>
                              <td style='padding:14px 22px 18px 22px;background:#fbfdff;border-top:1px solid #e6eefb;'>
                                <div style='font-family:Inter,system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;font-size:11px;line-height:1.6;color:#94a3b8;'>
                                  This is an automated message — please do not reply.
                                </div>
                              </td>
                            </tr>
                          </table>

                          <div style='height:18px;line-height:18px;font-size:18px;'>&nbsp;</div>

                          <div style='text-align:center;font-family:Inter,system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;font-size:11px;line-height:1.6;color:#94a3b8;'>
                            © {DateTime.UtcNow.Year} Starvia
                          </div>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>

            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }
    }
}