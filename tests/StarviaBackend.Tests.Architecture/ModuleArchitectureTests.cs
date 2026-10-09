using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NetArchTest.Rules;

namespace ModularMonolith.Tests.Architecture;

/// <summary>
/// Enforces the modular-monolith layering rules at build time. Every module gets its own thin
/// subclass (see <c>AccountsArchitectureTests</c>) that only supplies the module name and assemblies,
/// so the same guarantees hold for all modules and xUnit reports results per module.
/// </summary>
public abstract class ModuleArchitectureTests
{
    private const string ModulesRoot = "StarviaBackend.Modules";
    private static readonly string[] Layers = ["Core", "Application", "Infrastructure", "Api"];

    /// <summary>All modules of the solution. Add a new module name here to isolate it from the others.</summary>
    private static readonly string[] AllModules = ["Accounts", "Emails", "Media", "Platforms", "Posts"];

    /// <summary>
    /// Known, tolerated cross-module references (consumer module -> referenced module).
    /// Keep this list empty if possible; every entry is architectural debt.
    /// </summary>
    private static readonly HashSet<(string Consumer, string Referenced)> KnownCrossModuleDependencies =
    [
        // Emails.Application builds its messages from Accounts.Application commands/results.
        // TODO: move the shared contract to Accounts.Integrations / Shared.Abstractions.
        ("Emails", "Accounts"),
    ];

    private readonly string _module;
    private readonly Assembly _core;
    private readonly Assembly _application;
    private readonly Assembly _infrastructure;
    private readonly Assembly _api;

    protected ModuleArchitectureTests(
        string module,
        Assembly core,
        Assembly application,
        Assembly infrastructure,
        Assembly api)
    {
        _module = module;
        _core = core;
        _application = application;
        _infrastructure = infrastructure;
        _api = api;
    }

    private string CoreNamespace => Namespace("Core");
    private string ApplicationNamespace => Namespace("Application");
    private string InfrastructureNamespace => Namespace("Infrastructure");
    private string ApiNamespace => Namespace("Api");

    [Fact]
    public void Module_assemblies_should_contain_types_in_their_own_namespace()
    {
        // Guards against vacuous passes (e.g. wrong assembly marker => every rule trivially green).
        HasTypesIn(_core, CoreNamespace).Should().BeTrue($"{CoreNamespace} should contain types");
        HasTypesIn(_application, ApplicationNamespace).Should().BeTrue($"{ApplicationNamespace} should contain types");
        HasTypesIn(_infrastructure, InfrastructureNamespace).Should().BeTrue($"{InfrastructureNamespace} should contain types");
        HasTypesIn(_api, ApiNamespace).Should().BeTrue($"{ApiNamespace} should contain types");
    }

    [Fact]
    public void Core_should_not_depend_on_outer_layers()
    {
        var result = Types.InAssembly(_core)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Because(result));
    }

    [Fact]
    public void Application_should_not_depend_on_module_infrastructure_or_api()
    {
        var result = Types.InAssembly(_application)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Because(result));
    }

    [Fact]
    public void Infrastructure_should_not_depend_on_api()
    {
        var result = Types.InAssembly(_infrastructure)
            .ShouldNot()
            .HaveDependencyOnAny(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Because(result));
    }

    [Fact]
    public void Layers_should_not_depend_on_other_modules()
    {
        var failures = new List<string>();

        foreach (var (layer, assembly) in LayerAssemblies())
        {
            var forbidden = AllModules
                .Where(other => other != _module)
                .Where(other => !KnownCrossModuleDependencies.Contains((_module, other)))
                .SelectMany(other => Layers.Select(l => $"{ModulesRoot}.{other}.{l}"))
                .ToArray();

            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(forbidden)
                .GetResult();

            if (!result.IsSuccessful)
            {
                failures.Add($"{layer}: {string.Join(", ", result.FailingTypeNames ?? [])}");
            }
        }

        failures.Should().BeEmpty(
            "modules must communicate through messages/integration contracts, not by referencing each other's internals");
    }

    [Fact]
    public void Command_and_query_handlers_should_not_be_public()
    {
        foreach (var assembly in new[] { _application, _infrastructure })
        {
            var result = Types.InAssembly(assembly)
                .That()
                .HaveNameEndingWith("Handler")
                .Should()
                .NotBePublic()
                .GetResult();

            result.IsSuccessful.Should().BeTrue(Because(result));
        }
    }

    [Fact]
    public void Endpoints_should_not_be_public()
    {
        var result = Types.InAssembly(_api)
            .That()
            .HaveNameEndingWith("Endpoint")
            .Should()
            .NotBePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Because(result));
    }

    [Fact]
    public void DbContexts_should_live_in_infrastructure()
    {
        var result = Types.InAssembly(_infrastructure)
            .That()
            .Inherit(typeof(DbContext))
            .Should()
            .ResideInNamespaceStartingWith(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Because(result));

        foreach (var assembly in new[] { _core, _application, _api })
        {
            Types.InAssembly(assembly)
                .That()
                .Inherit(typeof(DbContext))
                .GetTypes()
                .Should()
                .BeEmpty("DbContexts belong only in the Infrastructure layer");
        }
    }

    private string Namespace(string layer) => $"{ModulesRoot}.{_module}.{layer}";

    private IEnumerable<(string Layer, Assembly Assembly)> LayerAssemblies()
    {
        yield return ("Core", _core);
        yield return ("Application", _application);
        yield return ("Infrastructure", _infrastructure);
        yield return ("Api", _api);
    }

    private static bool HasTypesIn(Assembly assembly, string @namespace) =>
        Types.InAssembly(assembly).That().ResideInNamespaceStartingWith(@namespace).GetTypes().Any();

    private static string Because(TestResult result) =>
        result.FailingTypeNames is null
            ? string.Empty
            : "these types break the rule: " + string.Join(", ", result.FailingTypeNames);
}
