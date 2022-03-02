using System;
using System.Runtime.Serialization;

namespace BluePrism.ClientServerResources.Core.Data
{
    [Serializable]
    [DataContract(Namespace = "bp")]
    public class SessionEndData
    {
        [DataMember]
        public Guid SessionId { get; set; }
        [DataMember]
        public string Status { get; set; }

        public SessionEndData(Guid sessId, string status)
        {
            SessionId = sessId;
            Status = status;
        }

        public string FinalStatusString => Status ?? string.Empty;
    }
}
