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
    public class RepositoryNamingTests : ArchitectureTestsBase
    {
        [Fact]
        public void Repositories_Should_EndWith_Repository()
        {
            var result = Types
                .InNamespace(RepositoryNamespace + ".Repositories")
                .Should()
                .HaveNameEndingWith("Repository")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Repositories_Should_Not_Be_Named_Service()
        {
            var result = Types
                .InNamespace(RepositoryNamespace + ".Repositories")
                .ShouldNot()
                .HaveNameEndingWith("Service")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Repositories_Should_Be_Public()
        {
            var result = Types
                .InNamespace(RepositoryNamespace + ".Repositories")
                .That()
                .HaveNameEndingWith("Repository")
                .Should()
                .BePublic()
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Repositories_Should_Not_Be_Static()
        {
            var result = Types
                .InNamespace(RepositoryNamespace + ".Repositories")
                .That()
                .HaveNameEndingWith("Repository")
                .ShouldNot()
                .BeStatic()
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Repositories_Should_Reside_In_Repository_Namespace()
        {
            var result = Types
                .InAssembly(Repository)
                .That()
                .HaveNameEndingWith("Repository")
                .Should()
                .ResideInNamespace(RepositoryNamespace + ".Repositories")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }
    }
}
