using System;
using System.Runtime.Serialization;

namespace BluePrism.ClientServerResources.Core.Exceptions
{
    [Serializable]
    public class InvalidInstructionalConnectionException : Exception
    {
        public InvalidInstructionalConnectionException(string message) : base(message)
        {
        }

        public InvalidInstructionalConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidInstructionalConnectionException()
        {
        }

        protected InvalidInstructionalConnectionException(SerializationInfo info, StreamingContext context)
           :base(info,context)
        {
        } 
    }
}
