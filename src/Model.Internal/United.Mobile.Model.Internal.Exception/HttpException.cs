using System.Net;

namespace System.Web
{
    public class HttpException : Exception
    {
        private readonly int httpStatusCode;
        private readonly string message;
        private readonly Exception exception;

        public HttpException(int httpStatusCode)
        {
            this.httpStatusCode = httpStatusCode;
        }

        public HttpException(string message)
        {   
            this.message = message;
        }
        public HttpException(Exception exception)
        {
            this.exception = exception;
        }

        public HttpException(HttpStatusCode httpStatusCode)
        {
            this.httpStatusCode = (int)httpStatusCode;
        }

        public HttpException(int httpStatusCode, string message) : base(message)
        {
            this.httpStatusCode = httpStatusCode;
        }

        public HttpException(HttpStatusCode httpStatusCode, string message) : base(message)
        {
            this.httpStatusCode = (int)httpStatusCode;
        }

        public HttpException(int httpStatusCode, string message, Exception inner) : base(message, inner)
        {
            this.httpStatusCode = httpStatusCode;
        }

        public HttpException(HttpStatusCode httpStatusCode, string message, Exception inner) : base(message, inner)
        {
            this.httpStatusCode = (int)httpStatusCode;
        }

        public int StatusCode { get { return this.httpStatusCode; } }
        public string Message { get { return this.message; } }
    }
}
