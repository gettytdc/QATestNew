using BluePrism.ClientServerResources.Core.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BluePrism.ClientServerResources.Core.Data
{
    [Serializable]
    [DataContract(Namespace = "bp")]
    public class SessionCreatedData
    {
        [DataMember]
        public SessionCreateState State { get; set; }
        // The new session ID or Empty if no session was created
        [DataMember]
        public Guid SessionId { get; set; }
        // The resource ID on which the session creation was done
        [DataMember]
        public Guid ResourceId { get; set; }
        // The ID of the process with which the session creation was done
        [DataMember]
        public Guid ProcessId { get; set; }
        // The message indicating the error which occurred on session creation
        [DataMember]
        public string ErrorMessage { get; set; }
        // Supplementary data about the session creation, usually detailed error messages
        [DataMember]
        public IDictionary<string, RunnerStatus> Data { get; set; }
        // The ID of the scheduled session which caused this session create request,
        // or zero if it is not the product of a scheduled session
        [DataMember]
        public int ScheduledSessionId { get; set; }
        [DataMember]
        public Guid UserId { get; set; }
        [DataMember]
        public object Tag { get; set; }

        public SessionCreatedData(SessionCreateState createState, Guid resourceId, Guid processId, Guid sessionId, int schedSessId, string errMsg, IDictionary<string, RunnerStatus> data, Guid userId, object tag)
        {
            State = createState;
            ResourceId = resourceId;
            ProcessId = processId;
            SessionId = sessionId;
            ScheduledSessionId = schedSessId;
            ErrorMessage = errMsg;
            Data = data;
            UserId = userId;
            Tag = tag;
        }
    }
}
