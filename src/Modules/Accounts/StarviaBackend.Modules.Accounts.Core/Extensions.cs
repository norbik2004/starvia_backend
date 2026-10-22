using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

[assembly:InternalsVisibleTo("StarviaBackend.Modules.Accounts.Infrastructure")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Accounts.Application")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Accounts.Api")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Accounts.Tests.Integration")]
namespace StarviaBackend.Modules.Accounts.Core;

internal static class Extensions
{
    /// <summary>Registers pure-domain services. Kept for symmetry with the other layers.</summary>
    public static IServiceCollection AddCore(this IServiceCollection services) => services;
}
