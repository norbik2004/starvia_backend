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
    public class WebDependencyTests : ArchitectureTestsBase
    {
        [Fact]
        public void Controllers_Should_ResideIn_Web()
        {
            var result = Types
                .InAssembly(Web)
                .That()
                .HaveNameEndingWith("Controller")
                .Should()
                .ResideInNamespace(WebNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Controllers_Should_Be_Public()
        {
            var result = Types
                .InAssembly(Web)
                .That()
                .HaveNameEndingWith("Controller")
                .Should()
                .BePublic()
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

    }
}
