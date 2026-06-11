using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Enums;

namespace Core.Application.DTO.Platform.Response
{
    public class PlatformResponse
    {
        public int Id { get; set; }
        public PlatformType Type { get; set; }
    }
}
