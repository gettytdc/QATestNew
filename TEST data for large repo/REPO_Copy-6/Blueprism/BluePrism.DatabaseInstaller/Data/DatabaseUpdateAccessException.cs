using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace BluePrism.DatabaseInstaller
{
    [Serializable]
    public class DatabaseUpdateAccessException : ApplicationException
    {
        public string ResourceReferenceProperty { get; set; }

        public DatabaseUpdateAccessException()
            : base() { }

        public DatabaseUpdateAccessException(string message)
            : base(message) { }

        public DatabaseUpdateAccessException(string message, Exception inner)
            : base(message, inner) { }

        protected DatabaseUpdateAccessException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
