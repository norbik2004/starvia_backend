using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ApplicationSettings
    {
        public required string BackendURL { get; set; }
        public required string FrontendURL { get; set; }
    }
}
