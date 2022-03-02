namespace BluePrism.Api.Setup.Common.Exceptions
{
    using System;

    public class InvalidWixXmlException : Exception
    {
        public InvalidWixXmlException(string message) : base(message)
        {
        }
        public InvalidWixXmlException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
