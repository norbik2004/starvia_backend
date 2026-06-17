using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ArchitectureTests.Shared
{
    public static class Assemblies
    {
        public static readonly Assembly CoreApplication =
            typeof(Core.Application.AssemblyReference).Assembly;

        public static readonly Assembly CoreDomain =
            typeof(Core.Domain.AssemblyReference).Assembly;

        public static readonly Assembly CoreInfrastructure =
            typeof(Core.Infrastructure.AssemblyReference).Assembly;

        public static readonly Assembly Repository =
            typeof(Repository.AssemblyReference).Assembly;

        public static readonly Assembly Service =
            typeof(Service.AssemblyReference).Assembly;

        public static readonly Assembly Web =
            typeof(Web.AssemblyReference).Assembly;
    }
}
