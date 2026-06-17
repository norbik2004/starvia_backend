using ArchitectureTests.Shared;
using FluentAssertions;
using NetArchTest.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchitectureTests.Dependencies
{
    public class RepositoryDependencyTests : ArchitectureTestsBase
    {
        [Fact]
        public void Repository_Should_Not_DependOn_Web()
        {
            var result = Types
                .InAssembly(Repository)
                .ShouldNot()
                .HaveDependencyOn(WebNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Repository_Should_Not_DependOn_Application()
        {
            var result = Types
                .InAssembly(Repository)
                .ShouldNot()
                .HaveDependencyOn(CoreApplicationNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Repository_Should_Not_DependOn_Service()
        {
            var result = Types
                .InAssembly(Repository)
                .ShouldNot()
                .HaveDependencyOn(ServiceNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Repository_Should_Not_DependOn_Application_Dtos()
        {
            var result = Types
                .InAssembly(Repository)
                .That()
                .HaveNameEndingWith("DTO")
                .ShouldNot()
                .HaveDependencyOn(CoreApplicationNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

    }
}
