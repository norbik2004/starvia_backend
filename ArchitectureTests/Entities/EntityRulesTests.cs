using ArchitectureTests.Shared;
using FluentAssertions;
using NetArchTest.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchitectureTests.Entities
{
    public class EntityRulesTests : ArchitectureTestsBase
    {
        [Fact]
        public void Entities_Should_Not_Be_Dto()
        {
            var result = Types
                .InAssembly(CoreDomain)
                .That()
                .ResideInNamespace(CoreDomainNamespace)
                .ShouldNot()
                .HaveNameEndingWith("DTO")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Entities_Should_Not_Be_Service()
        {
            var result = Types
                .InAssembly(CoreDomain)
                .That()
                .ResideInNamespace(CoreDomainNamespace)
                .ShouldNot()
                .HaveNameEndingWith("Service")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Entities_Should_Not_Be_Repository()
        {
            var result = Types
                .InAssembly(CoreDomain)
                .That()
                .ResideInNamespace(CoreDomainNamespace)
                .ShouldNot()
                .HaveNameEndingWith("Repository")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Entities_Should_Not_Be_Static()
        {
            var result = Types
                .InAssembly(CoreDomain)
                .That()
                .ResideInNamespace(CoreDomainNamespace + ".Entities")
                .ShouldNot()
                .BeStatic()
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

    }
}
