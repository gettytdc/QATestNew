namespace BluePrism.DocumentProcessing.Integration
{
    using System.Net;
    using Server.Domain.Models;

    public class InvalidResponseException : BluePrismException
    {
        public HttpStatusCode ResponseStatusCode { get; }

        public InvalidResponseException(HttpStatusCode responseStatusCode, string message)
            : base(message)
        {
            ResponseStatusCode = responseStatusCode;
        }
    }
}
