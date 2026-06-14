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
        protected static Assembly CoreApplication => Assemblies.CoreApplication;
    }
}
