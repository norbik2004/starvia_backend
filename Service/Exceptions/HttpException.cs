using System;

namespace Service.Exceptions
{
    public class HttpException : Exception
    {
        public double StatusCode { get; }

        public new string Message { get; }

        public object Error { get; internal set; }

        public HttpException(double subCode, int statusCode, string message, object error)
        {
            if (subCode < 0.0 || subCode >= 1.0)
            {
                throw new FormatException("sub code must be bigger than 0 and smaller than 1");
            }

            if (statusCode <= 0 || statusCode > 999999999)
            {
                throw new FormatException("status code must be bigger than 0 and smaller than 999999999");
            }

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message is required");
            }

            StatusCode = (double)statusCode + subCode;
            Message = message;
            Error = error;
        }
    }
}
