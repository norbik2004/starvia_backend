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
    public class ServiceDependencyTests : ArchitectureTestsBase
    {
        [Fact]
        public void Service_Should_Not_DependOn_Web()
        {
            var result = Types
                .InAssembly(Service)
                .ShouldNot()
                .HaveDependencyOn(WebNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Service_Should_Not_DependOn_Repository()
        {
            var result = Types
                .InAssembly(Service)
                .ShouldNot()
                .HaveDependencyOn(RepositoryNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

    }
}
