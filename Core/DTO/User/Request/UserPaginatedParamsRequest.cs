using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.User.Request
{
    public class UserPaginatedParamsRequest
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
