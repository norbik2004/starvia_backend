using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.API
{
    public class ApiEmailResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? Email { get; set; }
    }
}
