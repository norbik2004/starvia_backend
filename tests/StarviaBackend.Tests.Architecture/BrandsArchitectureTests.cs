using StarviaBackend.Modules.Brands.Api;
using ApplicationMarker = StarviaBackend.Modules.Brands.Application.AssemblyReference;
using CoreMarker = StarviaBackend.Modules.Brands.Core.AssemblyReference;
using InfrastructureMarker = StarviaBackend.Modules.Brands.Infrastructure.AssemblyReference;

namespace ModularMonolith.Tests.Architecture;

public sealed class BrandsArchitectureTests() : ModuleArchitectureTests(
    "Brands",
    typeof(CoreMarker).Assembly,
    typeof(ApplicationMarker).Assembly,
    typeof(InfrastructureMarker).Assembly,
    typeof(BrandsModule).Assembly);
