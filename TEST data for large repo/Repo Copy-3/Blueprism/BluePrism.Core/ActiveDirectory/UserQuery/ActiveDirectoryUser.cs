using System;
using System.Runtime.Serialization;

namespace BluePrism.Core.ActiveDirectory.UserQuery
{
    [Serializable, DataContract(Namespace = "bp")]
    public class ActiveDirectoryUser
    {
        [DataMember]
        private readonly string _userPrincipalName;
        [DataMember]
        private readonly string _sid;
        [DataMember]
        private readonly string _distinguishedName;
        [DataMember]
        private readonly bool _alreadyMapped;

        public string Sid { get => _sid; }

        public string DistinguishedName { get => _distinguishedName; }

        public string UserPrincipalName { get => _userPrincipalName; }

        public bool AlreadyMapped { get => _alreadyMapped; }

        public ActiveDirectoryUser(string userPrincipalName, string sid, string distinguishedName, bool alreadyMapped)
        {
            _userPrincipalName = userPrincipalName;
            _sid = sid;
            _distinguishedName = distinguishedName;
            _alreadyMapped = alreadyMapped;
        }                
    }

}
