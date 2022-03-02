using System;
using System.Runtime.Serialization;

namespace BluePrism.ClientServerResources.Core.Data
{
    [Serializable]
    [DataContract(Namespace = "bp")]
    public class SessionStartedData
    {
        [DataMember]
        public Guid SessionId { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public string UserMessage { get; set; }

        [DataMember]
        public Guid UserId { get; set; }

        [DataMember]
        public int SchedulerID { get; set; }

        public SessionStartedData(Guid sessId, string errmsg, Guid userId, string userMessage, int schedulerId)
        {
            SessionId = sessId;
            ErrorMessage = errmsg;
            UserMessage = userMessage;
            UserId = userId;
            SchedulerID = schedulerId;
        }
    }
}
