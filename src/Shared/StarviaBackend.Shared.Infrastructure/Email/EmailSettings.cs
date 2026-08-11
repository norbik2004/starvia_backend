using System;
using System.Collections.Generic;
using System.Text;

namespace StarviaBackend.Shared.Infrastructure.Email;

internal sealed class EmailSettings
{
    public required string Host { get; set; }
    public int Port { get; set; }
    public bool UseSSL { get; set; }
    public bool DefaultCredentials { get; set; }
    public required string EmailId { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required string Name { get; set; }
}
