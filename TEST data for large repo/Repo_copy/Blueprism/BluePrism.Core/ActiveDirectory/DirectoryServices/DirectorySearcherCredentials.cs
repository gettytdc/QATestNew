using BluePrism.Common.Security;
using System;
using System.Runtime.Serialization;

namespace BluePrism.Core.ActiveDirectory.DirectoryServices
{
    [Serializable, DataContract(Namespace = "bp")]
    public class DirectorySearcherCredentials
    {
        [DataMember]
        private readonly string _userName;

        [DataMember]
        private readonly SafeString _password;

        public DirectorySearcherCredentials(string username, SafeString password)
        {
            _userName = username;
            _password = password;
         }

        public string Username { get => _userName; }
        public SafeString Password { get => _password; }
    }
}