using StarviaBackend.Modules.Media.Api;
using ApplicationMarker = StarviaBackend.Modules.Media.Application.AssemblyReference;
using CoreMarker = StarviaBackend.Modules.Media.Core.AssemblyReference;
using InfrastructureMarker = StarviaBackend.Modules.Media.Infrastructure.AssemblyReference;

namespace ModularMonolith.Tests.Architecture;

public sealed class MediaArchitectureTests() : ModuleArchitectureTests(
    "Media",
    typeof(CoreMarker).Assembly,
    typeof(ApplicationMarker).Assembly,
    typeof(InfrastructureMarker).Assembly,
    typeof(MediaModule).Assembly);
