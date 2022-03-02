using System;

namespace BluePrism.ClientServerResources.Core.Events
{
    public delegate void SessionDeleteEventHandler(object sender, SessionDeleteEventArgs e);

    public class SessionDeleteEventArgs : BaseResourceEventArgs
    {
        /// <summary>
        /// Creates a new event args object.
        /// </summary>
        /// <param name="sessId">The ID of the deleted session</param>
        public SessionDeleteEventArgs(Guid sessId) : this(sessId, null, Guid.Empty, 0)
        {
        }

        public SessionDeleteEventArgs(Guid sessId, Guid userId) : this(sessId, null, userId, 0)
        {
        }

        public SessionDeleteEventArgs(Guid sessId, string errmsg, Guid userId, int schedId)
        {
            SessionId = sessId;
            ErrorMessage = errmsg;
            UserId = userId;
            ScheduledSessionId = schedId;
        }

        /// <summary>
        /// The ID of the scheduled session object which initiated the session - zero if
        /// no such object was referred to.
        /// </summary>
        public override bool FromScheduler => ScheduledSessionId > 0;

        public int ScheduledSessionId { get; }
        public Guid UserId { get; }
    }
}
