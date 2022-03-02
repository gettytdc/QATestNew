using System;

namespace BluePrism.ClientServerResources.Core.Events
{

    public delegate void SessionStartEventHandler(object sender, SessionStartEventArgs e);

    public class SessionStartEventArgs
        : BaseResourceEventArgs
    {
        public SessionStartEventArgs(
            Guid sessid = default(Guid),
            Guid userid = default(Guid),
            string errmsg = default(string),
            string usermsg = default(string),
            int schedId = default(int))
        {

            SessionId = sessid;
            UserId = userid;
            ErrorMessage = errmsg;
            UserMessage = usermsg;
            ScheduledSessionId = schedId;

        }

        /// <summary>
        /// The ID of the scheduled session object which initiated the session - zero if
        /// no such object was referred to.
        /// </summary>
        public int ScheduledSessionId { get; }

        public override bool FromScheduler => ScheduledSessionId > 0;

        public Guid UserId { get; }
    }
}
