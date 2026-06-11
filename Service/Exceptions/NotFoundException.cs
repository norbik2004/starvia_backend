using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Exceptions
{
    public class NotFoundException : HttpException
    {
        private const int statusCode = 404;

        public NotFoundException(string message)
            : base(0.0, 404, message, null)
        {
        }

        public NotFoundException(string message, object error)
            : base(0.0, 404, message, error)
        {
        }

        public NotFoundException(double subCode, string message)
            : base(subCode, 404, message, null)
        {
        }

        public NotFoundException(double subCode, string message, object error)
            : base(subCode, 404, message, error)
        {
        }
    }
}
