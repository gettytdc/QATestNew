namespace BluePrism.Api.Models.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class PagingTokenPropertyNotFoundException : Exception
    {
        public PagingTokenPropertyNotFoundException() { }

        public PagingTokenPropertyNotFoundException(string message) : base(message) { }

        public PagingTokenPropertyNotFoundException(string message, Exception innerException) : base(message, innerException) { }

        protected PagingTokenPropertyNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
