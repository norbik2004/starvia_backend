using System;
using System.Collections.Generic;
using System.Text;

namespace StarviaBackend.Modules.Emails.Application.Emails.Templates.Models;

public sealed record ConfirmAccountEmailTemplateModel(
    string Email, string UserName, string Link);
