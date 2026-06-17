using ArchitectureTests.Shared;
using FluentAssertions;
using NetArchTest.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchitectureTests.NamingConventions
{
    public class ServiceNamingTests : ArchitectureTestsBase
    {
        [Fact]
        public void Services_Should_EndWith_Service()
        {
            var result = Types
                .InNamespace(ServiceNamespace + ".Services")
                .Should()
                .HaveNameEndingWith("Service")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Services_Should_Not_Be_Named_Repository()
        {
            var result = Types
                .InAssembly(Service)
                .ShouldNot()
                .HaveNameEndingWith("Repository")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Services_Should_Not_Be_Named_Controller()
        {
            var result = Types
                .InAssembly(Service)
                .ShouldNot()
                .HaveNameEndingWith("Controller")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Services_Should_Be_Public()
        {
            var result = Types
                .InAssembly(Service)
                .That()
                .HaveNameEndingWith("Service")
                .Should()
                .BePublic()
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Services_Should_Not_Be_Static()
        {
            var result = Types
                .InAssembly(Service)
                .That()
                .HaveNameEndingWith("Service")
                .ShouldNot()
                .BeStatic()
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Services_Should_Reside_In_Service_Namespace()
        {
            var result = Types
                .InAssembly(Service)
                .That()
                .HaveNameEndingWith("Service")
                .Should()
                .ResideInNamespace(ServiceNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }
    }
}
