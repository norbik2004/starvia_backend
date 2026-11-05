using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

[assembly:InternalsVisibleTo("StarviaBackend.Modules.Ai.Infrastructure")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Ai.Application")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Ai.Api")]
[assembly:InternalsVisibleTo("StarviaBackend.Modules.Ai.Tests.Integration")]
namespace StarviaBackend.Modules.Ai.Core;

internal static class Extensions
{
    public static IServiceCollection AddCore(this IServiceCollection services) => services;
}
