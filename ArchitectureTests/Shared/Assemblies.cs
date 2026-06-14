using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ArchitectureTests.Shared
{
    public static class Assemblies
    {
        public static readonly Assembly CoreApplication =
        typeof(Core.Application.AssemblyReference).Assembly;
    }
}
