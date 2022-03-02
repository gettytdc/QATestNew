namespace BluePrism.Api.Models.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class CommaDelimitedCollectionConstructorMissingException : Exception
    {
        public CommaDelimitedCollectionConstructorMissingException()
        {
        }

        protected CommaDelimitedCollectionConstructorMissingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
