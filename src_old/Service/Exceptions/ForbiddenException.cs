using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Exceptions
{
    public class ForbiddenException : HttpException
    {
        private const int statusCode = 403;

        public ForbiddenException(string message)
            : base(0.0, 403, message, null)
        {
        }

        public ForbiddenException(string message, object error)
            : base(0.0, 403, message, error)
        {
        }

        public ForbiddenException(double subCode, string message)
            : base(subCode, 403, message, null)
        {
        }

        public ForbiddenException(double subCode, string message, object error)
            : base(subCode, 403, message, error)
        {
        }
    }
}
