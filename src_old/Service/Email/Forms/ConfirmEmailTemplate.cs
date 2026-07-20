using System;
using Core.Application.DTO.Email.Models;
namespace Service.Email.Forms
{
    public static class ConfirmEmailTemplate
    {
        public static string GenerateUserMessageTemplate(
            ConfirmEmailRequest request,
            string backendURL)
        {
            var url =
                $"{backendURL}/api/Account/confirm-email?userId={request.UserId}&token={Uri.EscapeDataString(request.Token)}";

            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1'>
  <meta name='x-apple-disable-message-reformatting'>
  <title>Confirm your email</title>
</head>
<body style='margin:0;padding:0;background-color:#ffffff;'>
  <!-- Preheader (hidden) -->
  <div style='display:none;max-height:0;overflow:hidden;opacity:0;color:transparent;mso-hide:all;'>
    Confirm your Starvia account email address.
  </div>
  <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='width:100%;border-collapse:collapse;background-color:#ffffff;'>
    <tr>
      <td align='center' style='padding:40px 16px;background:#ffffff;'>
        <!-- Outer container -->
        <table role='presentation' width='640' cellspacing='0' cellpadding='0' border='0' style='width:640px;max-width:100%;border-collapse:separate;border-spacing:0;'>
          <tr>
            <td style='padding:0;'>
              <!-- Hero / gradient background -->
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
                          <!-- Card -->
                          <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0'
                                 style='width:100%;border-collapse:separate;border-spacing:0;background:#ffffff;border:1px solid #dbe7f7;border-radius:16px;overflow:hidden;box-shadow:0 18px 48px rgba(15,23,42,0.10);'>
                            <tr>
                              <td style='padding:26px 22px 18px 22px;text-align:left;background:linear-gradient(135deg,#0b63ce 0%, #1f86ff 55%, #2b5cff 100%);'>
                                <div style='font-family:Inter,system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;font-size:22px;line-height:1.25;color:#ffffff;font-weight:800;'>
                                  Confirm your email
                                </div>
                                <div style='margin-top:6px;font-family:Inter,system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;font-size:14px;line-height:1.5;color:rgba(255,255,255,0.88);'>
                                  One quick step to finish setting up your account.
                                </div>
                              </td>
                            </tr>
                            <tr>
                              <td style='padding:22px 22px 10px 22px;text-align:left;'>
                                <div style='font-family:Inter,system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;font-size:15px;line-height:1.7;color:#0f172a;'>
                                  Thanks for creating an account.
                                </div>
                                <div style='margin-top:10px;font-family:Inter,system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;font-size:14px;line-height:1.7;color:#475569;'>
                                  Please confirm your email address by clicking the button below.
                                </div>
                                <!-- Button -->
                                <table role='presentation' cellspacing='0' cellpadding='0' border='0' style='margin-top:18px;border-collapse:separate;'>
                                  <tr>
                                    <td align='center' style='border-radius:12px;background:linear-gradient(135deg,#0b63ce 0%, #1f86ff 55%, #2b5cff 100%);'>
                                      <a href='{url}'
                                         style='display:inline-block;padding:13px 18px;font-family:Inter,system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;font-size:14px;line-height:1.2;font-weight:800;color:#ffffff;text-decoration:none;border-radius:12px;'>
                                        Confirm Email
                                      </a>
                                    </td>
                                  </tr>
                                </table>
                                <!-- Fallback link -->
                                <div style='margin-top:16px;padding:12px 12px;border-radius:12px;background:#f6f9ff;border:1px solid #e3edff;'>
                                  <div style='font-family:Inter,system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;font-size:12px;line-height:1.6;color:#475569;'>
                                    If the button doesn’t work, copy and paste this link into your browser:
                                  </div>
                                  <div style='margin-top:6px;font-family:Consolas,ui-monospace,SFMono-Regular,Menlo,Monaco,monospace;font-size:12px;line-height:1.55;color:#0b63ce;word-break:break-all;'>
                                    <a href='{url}' style='color:#0b63ce;text-decoration:underline;'> {url} </a>
                                  </div>
                                </div>
                                <div style='margin-top:14px;font-family:Inter,system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;font-size:12px;line-height:1.6;color:#64748b;'>
                                  If you didn’t create this account, you can safely ignore this email.
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
                          <!-- Spacer -->
                          <div style='height:18px;line-height:18px;font-size:18px;'>&nbsp;</div>
                          <!-- Footer -->
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