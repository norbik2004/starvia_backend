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
        public static string GenerateUserMessageTemplate(ConfirmEmailRequest request, string backendURL)
        {
            var url = $"{backendURL}/api/Account/confirm-email?userId={request.UserId}&token={Uri.EscapeDataString(request.Token)}";

            return $@"
                <html>
                    <body>
                        <h1>Confirm your email</h1>
                        <p>Please click the link below to confirm your email address:</p>
                        <a href='{url}'>Confirm Email</a>
                    </body>
                </html>
            ";
        }
    }
}
