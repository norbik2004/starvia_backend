using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Exceptions
{
    public class BadRequestException : HttpException
    {

        public BadRequestException(string message)
            : base(0.0, 400, message, null)
        {
        }

        public BadRequestException(string message, object error)
            : base(0.0, 400, message, error)
        {
        }

        public BadRequestException(double subCode, string message)
            : base(subCode, 400, message, null)
        {
        }

        public BadRequestException(double subCode, string message, object error)
            : base(subCode, 400, message, error)
        {
        }
    }
}
