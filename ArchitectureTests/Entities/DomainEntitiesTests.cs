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
    public class DomainEntitiesTests : ArchitectureTestsBase
    {
        [Fact]
        public void DomainEntities_Should_Not_DependOn_Infrastructure()
        {
            var result = Types
                .InAssembly(CoreDomain)
                .That()
                .ResideInNamespace(CoreDomainNamespace)
                .ShouldNot()
                .HaveDependencyOn(CoreInfrastructureNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void DomainEntities_Should_Be_Public()
        {
            var result = Types
                .InAssembly(CoreDomain)
                .That()
                .ResideInNamespace(CoreDomainNamespace)
                .Should()
                .BePublic()
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void DomainEntities_Should_Not_DependOn_Dtos()
        {
            var result = Types
                .InAssembly(CoreDomain)
                .That()
                .HaveNameEndingWith("DTO")
                .ShouldNot()
                .HaveDependencyOn(CoreApplicationNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void DomainEntities_Should_Not_DependOn_Application()
        {
            var result = Types
                .InAssembly(CoreDomain)
                .That()
                .ResideInNamespace(CoreDomainNamespace)
                .ShouldNot()
                .HaveDependencyOn(CoreApplicationNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void DomainEntities_Should_Not_DependOn_Web()
        {
            var result = Types
                .InAssembly(CoreDomain)
                .That()
                .ResideInNamespace(CoreDomainNamespace)
                .ShouldNot()
                .HaveDependencyOn(WebNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }
    }
}
