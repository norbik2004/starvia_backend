using Microsoft.Extensions.DependencyInjection;

namespace StarviaBackend.Modules.Accounts.Core;

internal static class Extensions
{
    /// <summary>Registers pure-domain services. Kept for symmetry with the other layers.</summary>
    public static IServiceCollection AddCore(this IServiceCollection services) => services;
}
