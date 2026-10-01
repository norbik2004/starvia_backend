using System;
using System.Collections.Generic;
using System.Text;

namespace StarviaBackend.Shared.Abstractions.Email;

public enum EmailType
{
    WelcomingEmail,
    ConfirmAccountEmail,
    ResetPasswordEmail,
}
