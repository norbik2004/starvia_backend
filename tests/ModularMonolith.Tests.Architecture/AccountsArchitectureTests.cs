using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Accounts.Api;
using NetArchTest.Rules;
using CoreMarker = StarviaBackend.Modules.Accounts.Core.AssemblyReference;
using AppMarker = StarviaBackend.Modules.Accounts.Application.AssemblyReference;
using InfraMarker = StarviaBackend.Modules.Accounts.Infrastructure.AssemblyReference;

namespace ModularMonolith.Tests.Architecture;

/// <summary>
/// Enforces the modular-monolith layering rules at build time. Copy these when adding a module
/// (swap the namespaces/assemblies) so the same guarantees hold everywhere.
/// </summary>
public sealed class AccountsArchitectureTests
{
    private const string Core = "StarviaBackend.Modules.Accounts.Core";
    private const string Application = "StarviaBackend.Modules.Accounts.Application";
    private const string Infrastructure = "StarviaBackend.Modules.Accounts.Infrastructure";
    private const string Api = "StarviaBackend.Modules.Accounts.Api";

    private static readonly Assembly CoreAssembly = typeof(CoreMarker).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(AppMarker).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(InfraMarker).Assembly;
    private static readonly Assembly ApiAssembly = typeof(AccountsModule).Assembly;

    [Fact]
    public void Core_should_not_depend_on_outer_layers()
    {
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(Application, Infrastructure, Api)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Because(result));
    }

    [Fact]
    public void Application_should_not_depend_on_module_infrastructure_or_api()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(Infrastructure, Api)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Because(result));
    }

    [Fact]
    public void Command_and_query_handlers_should_not_be_public()
    {
        foreach (var assembly in new[] { ApplicationAssembly, InfrastructureAssembly })
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
        var result = Types.InAssembly(ApiAssembly)
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
        var result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .Inherit(typeof(DbContext))
            .Should()
            .ResideInNamespaceStartingWith(Infrastructure)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Because(result));

        foreach (var assembly in new[] { CoreAssembly, ApplicationAssembly, ApiAssembly })
        {
            Types.InAssembly(assembly)
                .That()
                .Inherit(typeof(DbContext))
                .GetTypes()
                .Should()
                .BeEmpty("DbContexts belong only in the Infrastructure layer");
        }
    }

    private static string Because(TestResult result) =>
        result.FailingTypeNames is null
            ? string.Empty
            : "these types break the rule: " + string.Join(", ", result.FailingTypeNames);
}
