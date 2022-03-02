using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace BluePrism.DatabaseInstaller
{
    [Serializable]
    public class DatabaseInstallerException : Exception
    {
        public string ResourceReferenceProperty { get; set; }
        public Exception AssociatedException { get; set; }

        public DatabaseInstallerException()
            : base() { }


        public DatabaseInstallerException(string message)
            : base(message) { }

        public DatabaseInstallerException(string message, Exception inner)
            : base(message, inner) { }

        protected DatabaseInstallerException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
       

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }


    }
}
