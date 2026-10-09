using StarviaBackend.Modules.Platforms.Api;
using ApplicationMarker = StarviaBackend.Modules.Platforms.Application.AssemblyReference;
using CoreMarker = StarviaBackend.Modules.Platforms.Core.AssemblyReference;
using InfrastructureMarker = StarviaBackend.Modules.Platforms.Infrastructure.AssemblyReference;

namespace ModularMonolith.Tests.Architecture;

public sealed class PlatformsArchitectureTests() : ModuleArchitectureTests(
    "Platforms",
    typeof(CoreMarker).Assembly,
    typeof(ApplicationMarker).Assembly,
    typeof(InfrastructureMarker).Assembly,
    typeof(PlatformsModule).Assembly);
