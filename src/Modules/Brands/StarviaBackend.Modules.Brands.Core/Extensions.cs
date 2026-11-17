using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("StarviaBackend.Modules.Brands.Infrastructure")]
[assembly: InternalsVisibleTo("StarviaBackend.Modules.Brands.Application")]
[assembly: InternalsVisibleTo("StarviaBackend.Modules.Brands.Api")]
[assembly: InternalsVisibleTo("StarviaBackend.Modules.Brands.Tests.Integration")]
namespace StarviaBackend.Modules.Brands.Core;

internal static class Extensions
{
    /// <summary>Registers pure-domain services. Kept for symmetry with the other layers.</summary>
    public static IServiceCollection AddCore(this IServiceCollection services) => services;
}
