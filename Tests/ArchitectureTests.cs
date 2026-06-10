using NetArchTest.Rules;
using Xunit;

namespace tr_tests
{
    public class ArchitectureTests
    {
        private const string WebNamespace = "tr_backend";
        private const string CoreNamespace = "tr_core";
        private const string RepositoryNamespace = "tr_repository";
        private const string ServiceNamespace = "tr_service";

        [Fact]
        public void Core_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange
            var assembly = typeof(tr_core.ApplicationSettings).Assembly;

            var otherProjects = new[]
            {
                WebNamespace,
                RepositoryNamespace,
                ServiceNamespace,
            };

            // Act
            var result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(otherProjects)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, "Core should not depend on other layers");
        }

        [Fact]
        public void Repository_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange
            var assembly = typeof(tr_repository.TrDbContext).Assembly;

            var otherProjects = new[]
            {
                WebNamespace,
                ServiceNamespace,
            };

            // Act
            var result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(otherProjects)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, "Repository should not depend on other layers");
        }

        [Fact]
        public void Service_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange
            var assembly = typeof(tr_service.Email.EmailSender).Assembly;

            var otherProjects = new[]
            {
                WebNamespace,
            };

            // Act
            var result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(otherProjects)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, "Service should not depend on other layers");
        }
    }
}