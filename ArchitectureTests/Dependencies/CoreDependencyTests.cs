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
    public class CoreDependencyTests : ArchitectureTestsBase
    {


        [Fact]
        public void CoreDomain_Should_Not_HaveDependencyOnOtherProjects()
        {
            //Arrange
            var assembly = CoreDomain;

            var otherProjects = new[]
            {
                CoreApplicationNamespace,
                CoreInfrastructureNamespace,
                RepositoryNamespace,
                ServiceNamespace,
                WebNamespace
            };

            //Act
            var testResult = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAll(otherProjects)
                .GetResult();

            //Assert
            testResult.IsSuccessful.Should().BeTrue();
        }


        [Fact]
        public void CoreApplication_Should_Not_HaveDependencyOnOtherProjects()
        {
            //Arrange
            var assembly = CoreApplication;

            var otherProjects = new[]
            {
                CoreApplicationNamespace,
                CoreInfrastructureNamespace,
                RepositoryNamespace,
                ServiceNamespace,
                WebNamespace
            };

            //Act
            var testResult = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAll(otherProjects)
                .GetResult();

            //Assert
            testResult.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void CoreInfrastructure_Should_Not_HaveDependencyOnOtherProjects()
        {
            //Arrange
            var assembly = CoreInfrastructure;

            var otherProjects = new[]
            {
                RepositoryNamespace,
                ServiceNamespace,
                WebNamespace
            };

            //Act
            var testResult = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAll(otherProjects)
                .GetResult();

            //Assert
            testResult.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void CoreApplication_Should_Not_DependOn_EntityFramework()
        {
            var result = Types
                .InAssembly(CoreApplication)
                .ShouldNot()
                .HaveDependencyOn("Microsoft.EntityFrameworkCore")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void CoreApplication_Should_Not_DependOn_AspNetCore()
        {
            var result = Types
                .InAssembly(CoreApplication)
                .ShouldNot()
                .HaveDependencyOn("Microsoft.AspNetCore")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void CoreDomain_Should_Not_Use_Dtos()
        {
            var result = Types
                .InAssembly(CoreDomain)
                .That()
                .HaveNameEndingWith("DTO")
                .ShouldNot()
                .HaveDependencyOnAny(CoreApplicationNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }
    }
}
