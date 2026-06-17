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
    public class CoreNamingTests : ArchitectureTestsBase
    {
        [Fact]
        public void Application_Should_Not_Contain_Entities_In_Names()
        {
            var result = Types
                .InAssembly(CoreApplication)
                .That()
                .ResideInNamespace(CoreApplicationNamespace)
                .ShouldNot()
                .HaveNameEndingWith("Entity")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Infrastructure_Should_Not_Contain_Controllers()
        {
            var result = Types
                .InAssembly(CoreInfrastructure)
                .ShouldNot()
                .HaveNameEndingWith("Controller")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Domain_Should_Not_Contain_Dto()
        {
            var result = Types
                .InAssembly(CoreDomain)
                .ShouldNot()
                .HaveNameEndingWith("Dto")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Interfaces_Should_StartWith_I()
        {
            var result = Types
                .InAssemblies(new[] { CoreDomain, CoreApplication, CoreInfrastructure })
                .That()
                .AreInterfaces()
                .Should()
                .HaveNameStartingWith("I")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }
    }
}
