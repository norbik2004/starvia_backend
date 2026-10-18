using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

[assembly:InternalsVisibleTo("StarviaBackend.Modules.Calendar.Infrastructure")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Calendar.Application")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Calendar.Api")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Calendar.Tests.Integration")]
namespace StarviaBackend.Modules.Calendar.Core;

internal static class Extensions
{
    /// <summary>Registers pure-domain services. Kept for symmetry with the other layers.</summary>
    public static IServiceCollection AddCore(this IServiceCollection services) => services;
}
