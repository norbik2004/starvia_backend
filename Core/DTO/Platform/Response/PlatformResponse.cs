using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Enums;

namespace Core.DTO.Platform.Response
{
    public class PlatformResponse
    {
        public int Id { get; set; }
        public PlatformType Type { get; set; }
    }
}
