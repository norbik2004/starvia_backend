using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

[assembly:InternalsVisibleTo("StarviaBackend.Modules.Emails.Infrastructure")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Emails.Application")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Emails.Api")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Emails.Tests.Integration")]
namespace StarviaBackend.Modules.Emails.Core;

internal static class Extensions
{
    /// <summary>Registers pure-domain services. Kept for symmetry with the other layers.</summary>
    public static IServiceCollection AddCore(this IServiceCollection services) => services;
}
