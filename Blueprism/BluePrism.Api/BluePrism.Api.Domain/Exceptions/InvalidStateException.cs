namespace BluePrism.Api.Domain.Exceptions
{
    using System;

    public class InvalidStateException : Exception
    {
        public InvalidStateException(string message) : base(message) { }
    }
}
