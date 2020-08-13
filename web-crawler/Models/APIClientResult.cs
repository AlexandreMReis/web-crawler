using System.Net;

namespace web_crawler.Models
{
    /// <summary>
    /// The API result class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class APIClientResult<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether is success
        /// </summary>
        public bool Success
        {
            get
            {
                return this.ExceptionStatus == WebExceptionStatus.Success;
            }
        }

        /// <summary>
        /// Gets or sets the HTTP status code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the exception status
        /// </summary>
        public WebExceptionStatus ExceptionStatus { get; set; }

        /// <summary>
        /// Gets or sets the data
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Gets or sets the error data
        /// </summary>
        public object ErrorData { get; set; }

        /// <summary>
        /// The default constructor
        /// </summary>
        public APIClientResult()
        {
            this.ExceptionStatus = WebExceptionStatus.Success;
        }
    }
}
