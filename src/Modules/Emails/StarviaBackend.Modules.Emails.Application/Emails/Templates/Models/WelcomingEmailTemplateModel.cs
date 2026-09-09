using System;
using System.Collections.Generic;
using System.Text;

namespace StarviaBackend.Modules.Emails.Application.Emails.Templates.Models;

public sealed record WelcomingEmailTemplateModel(string UserName, string Link);
