using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Exceptions
{
    public class UnauthorizedException : HttpException
    {
        private const int statusCode = 401;

        public UnauthorizedException(string message)
            : base(0.0, 401, message, null)
        {
        }

        public UnauthorizedException(string message, object error)
            : base(0.0, 401, message, error)
        {
        }

        public UnauthorizedException(double subCode, string message)
            : base(subCode, 401, message, null)
        {
        }

        public UnauthorizedException(double subCode, string message, object error)
            : base(subCode, 401, message, error)
        {
        }
    }
}
