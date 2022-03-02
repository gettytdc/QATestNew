using System;
using System.Runtime.Serialization;

namespace BluePrism.ClientServerResources.Core.Data
{
    [Serializable]
    [DataContract(Namespace = "bp")]
    public class SessionDeletedData
    {
        [DataMember]
        public Guid SessionId { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public Guid UserId { get; set; }

        [DataMember]
        public int ScheduleId { get; set; }

        public SessionDeletedData(Guid sessId, string errmsg, Guid userId, int schedId)
        {
            SessionId = sessId;
            ErrorMessage = errmsg;
            UserId = userId;
            ScheduleId = schedId;
        }

        public bool Success => ErrorMessage is null;
    }
}
