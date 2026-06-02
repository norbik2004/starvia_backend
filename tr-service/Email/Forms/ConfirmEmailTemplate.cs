using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.DTO.Email.Models;

namespace tr_service.Email.Forms
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
<html>
<head>
    <meta charset='UTF-8'>
    <title>Confirm Email</title>
</head>
<body style='margin:0;padding:0;background-color:#f3f8ff;font-family:Segoe UI,Arial,sans-serif;'>

    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color:#f3f8ff;padding:40px 20px;'>
        <tr>
            <td align='center'>

                <table role='presentation' width='600' cellspacing='0' cellpadding='0' border='0'
                       style='background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 12px rgba(0,0,0,0.08);'>

                    <tr>
                        <td style='background:linear-gradient(135deg,#0078D4,#1890FF);padding:32px;text-align:center;'>
                            <h1 style='margin:0;color:#ffffff;font-size:28px;font-weight:600;'>
                                Confirm your email
                            </h1>
                        </td>
                    </tr>

                    <tr>
                        <td style='padding:40px 32px;'>

                            <p style='margin:0 0 20px 0;color:#1f2937;font-size:16px;line-height:24px;'>
                                Thank you for creating an account.
                            </p>

                            <p style='margin:0 0 32px 0;color:#4b5563;font-size:16px;line-height:24px;'>
                                Please confirm your email address by clicking the button below.
                            </p>

                            <table role='presentation' cellspacing='0' cellpadding='0' border='0' align='center'>
                                <tr>
                                    <td style='border-radius:8px;background:#0078D4;'>
                                        <a href='{url}'
                                           style='display:inline-block;padding:14px 32px;color:#ffffff;text-decoration:none;font-size:16px;font-weight:600;border-radius:8px;'>
                                            Confirm Email
                                        </a>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <tr>
                        <td style='background:#f8fafc;padding:24px;text-align:center;border-top:1px solid #e5e7eb;'>
                            <p style='margin:0;color:#94a3b8;font-size:12px;'>
                                This is an automated message. Please do not reply.
                            </p>
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