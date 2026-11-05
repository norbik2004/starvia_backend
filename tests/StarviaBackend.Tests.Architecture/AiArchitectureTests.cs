using FluentAssertions;
using NetArchTest.Rules;
using StarviaBackend.Modules.Ai.Api;
using ApplicationMarker = StarviaBackend.Modules.Ai.Application.AssemblyReference;
using CoreMarker = StarviaBackend.Modules.Ai.Core.AssemblyReference;
using InfrastructureMarker = StarviaBackend.Modules.Ai.Infrastructure.AssemblyReference;

namespace ModularMonolith.Tests.Architecture;

public sealed class AiArchitectureTests() : ModuleArchitectureTests(
    "Ai",
    typeof(CoreMarker).Assembly,
    typeof(ApplicationMarker).Assembly,
    typeof(InfrastructureMarker).Assembly,
    typeof(AiModule).Assembly)
{
    /// <summary>
    /// Which provider writes a text is decided by the registrations in Infrastructure (Google AI Studio, with
    /// OpenRouter kept apart for images). The use cases only know the vendor-neutral chat client, so a provider
    /// library imported there would be a way around that decision.
    /// </summary>
    [Fact]
    public void Use_cases_should_not_depend_on_an_AI_provider_library()
    {
        var result = Types.InAssembly(typeof(ApplicationMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Google.GenAI", "OpenAI", "System.ClientModel")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            result.FailingTypeNames is null
                ? string.Empty
                : "these types reach for a provider library: " + string.Join(", ", result.FailingTypeNames));
    }
}
