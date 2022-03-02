using System;

namespace BluePrism.ClientServerResources.Core.Events
{

    public delegate void SessionStopEventHandler(object sender, SessionStopEventArgs e);

    public class SessionStopEventArgs : BaseResourceEventArgs
    {

        public SessionStopEventArgs(Guid sessId, string errmsg, int schedId)
        {
            SessionId = sessId;
            ErrorMessage = errmsg;
            ScheduledSessionId = schedId;
        }

        /// <summary>
        /// The ID of the scheduled session object which initiated the session - zero if
        /// no such object was referred to.
        /// </summary>
        public int ScheduledSessionId { get; }
        public override bool FromScheduler => ScheduledSessionId > 0;        
    }
}
