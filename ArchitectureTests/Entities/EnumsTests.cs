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
    public class EnumsTests : ArchitectureTestsBase
    {
        [Fact]
        public void Enums_Should_Only_Exist_In_Domain()
        {
            var result = Types
                .InAssemblies(new[] { CoreDomain, CoreApplication, CoreInfrastructure, Repository, Service, Web })
                .That()
                .HaveNameEndingWith("Type")
                .Or()
                .HaveNameEndingWith("Status")
                .Should()
                .ResideInNamespace(CoreDomainNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Enums_Should_Not_Be_Empty()
        {
            var enums = CoreDomain
                .GetTypes()
                .Where(t => t.IsEnum && t.Namespace == CoreDomainNamespace);

            foreach (var enumType in enums)
            {
                var values = Enum.GetValues(enumType);
                values.Length.Should().BeGreaterThan(0, $"{enumType.Name} should not be empty");
            }
        }

        [Fact]
        public void Enums_Should_Not_Have_Too_Many_Values()
        {
            var enums = CoreDomain
                .GetTypes()
                .Where(t => t.IsEnum && t.Namespace == CoreDomainNamespace);

            foreach (var enumType in enums)
            {
                var valuesCount = Enum.GetValues(enumType).Length;

                valuesCount.Should().BeLessThan(50,
                    $"{enumType.Name} looks like it should be refactored (too many values)");
            }
        }
    }
}
