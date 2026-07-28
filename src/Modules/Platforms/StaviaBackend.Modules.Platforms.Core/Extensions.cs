using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

[assembly:InternalsVisibleTo("StarviaBackend.Modules.Platforms.Infrastructure")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Platforms.Application")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Platforms.Api")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Platforms.Tests.Integration")]
namespace StarviaBackend.Modules.Platforms.Core;

internal static class Extensions
{
    /// <summary>Registers pure-domain services. Kept for symmetry with the other layers.</summary>
    public static IServiceCollection AddCore(this IServiceCollection services) => services;
}
