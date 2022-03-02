using System;
using System.Runtime.Serialization;

namespace BluePrism.ClientServerResources.Core.Data
{
    [Serializable]
    [DataContract(Namespace = "bp")]
    public class SessionStopData
    {
        [DataMember]
        public Guid SessionId { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public int ScheduleId { get; set; }

        public SessionStopData(Guid sessId, string errmsg, int schedId)
        {
            SessionId = sessId;
            ErrorMessage = errmsg;
            ScheduleId = schedId;
        }

    }
}
