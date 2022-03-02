using System;
using BluePrism.ClientServerResources.Core.Data;
using System.Runtime.Serialization;

namespace BluePrism.ClientServerResources.Core.Exceptions
{
    [Serializable]
    public class TokenValidationException
        : Exception
    {
        public TokenValidationInfo ValidationResult { get; }


        public TokenValidationException(TokenValidationInfo result)
        {
            ValidationResult = result ?? throw new ArgumentNullException(nameof(result));
        }

        protected TokenValidationException(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
        }


    }
}
