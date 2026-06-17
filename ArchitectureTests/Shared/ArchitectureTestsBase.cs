using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ArchitectureTests.Shared
{
    public abstract class ArchitectureTestsBase
    {
        protected const string CoreApplicationNamespace = "Core.Application";
        protected const string CoreInfrastructureNamespace = "Core.Infrastructure";
        protected const string CoreDomainNamespace = "Core.Domain";
        protected const string RepositoryNamespace = "Repository";
        protected const string ServiceNamespace = "Service";
        protected const string WebNamespace = "Web";

        protected static Assembly CoreApplication => Assemblies.CoreApplication;
        protected static Assembly CoreDomain => Assemblies.CoreDomain;
        protected static Assembly CoreInfrastructure => Assemblies.CoreInfrastructure;
        protected static Assembly Repository => Assemblies.Repository;
        protected static Assembly Service => Assemblies.Service;
        protected static Assembly Web => Assemblies.Web;
    }
}
