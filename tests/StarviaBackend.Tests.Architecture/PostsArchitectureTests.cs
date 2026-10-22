using StarviaBackend.Modules.Posts.Api;
using ApplicationMarker = StarviaBackend.Modules.Posts.Application.AssemblyReference;
using CoreMarker = StarviaBackend.Modules.Posts.Core.AssemblyReference;
using InfrastructureMarker = StarviaBackend.Modules.Posts.Infrastructure.AssemblyReference;

namespace ModularMonolith.Tests.Architecture;

public sealed class PostsArchitectureTests() : ModuleArchitectureTests(
    "Posts",
    typeof(CoreMarker).Assembly,
    typeof(ApplicationMarker).Assembly,
    typeof(InfrastructureMarker).Assembly,
    typeof(PostsModule).Assembly);
