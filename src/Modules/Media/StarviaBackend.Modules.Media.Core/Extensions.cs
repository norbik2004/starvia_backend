using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("StarviaBackend.Modules.Media.Infrastructure")]
[assembly: InternalsVisibleTo("StarviaBackend.Modules.Media.Application")]
[assembly: InternalsVisibleTo("StarviaBackend.Modules.Media.Api")]
[assembly: InternalsVisibleTo("StarviaBackend.Modules.Media.Tests.Integration")]
namespace StarviaBackend.Modules.MediaFiles.Core;

internal static class Extensions
{
    /// <summary>Registers pure-domain services. Kept for symmetry with the other layers.</summary>
    public static IServiceCollection AddCore(this IServiceCollection services) => services;
}
