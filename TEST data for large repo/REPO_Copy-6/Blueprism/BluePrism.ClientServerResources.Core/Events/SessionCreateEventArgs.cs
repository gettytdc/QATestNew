using System;
using System.Collections.Generic;
using BluePrism.ClientServerResources.Core.Enums;

namespace BluePrism.ClientServerResources.Core.Events
{
    /// <summary>
    /// Delegate to handle Session End events
    /// </summary>
    public delegate void SessionCreateEventHandler(object sender, SessionCreateEventArgs e);

    /// <summary>
    /// Class to encapsulate data regarding the creation of a session, or the failure
    /// to create a requested session.
    /// </summary>
    public class SessionCreateEventArgs : BaseResourceEventArgs
    {

        /// <summary>
        /// Common constructor, setting all members.
        /// </summary>
        /// <param name="createState">The state of the session create event.</param>
        /// <param name="resourceId">The ID of the resource on which session create
        /// was attempted</param>
        /// <param name="processId">The ID of the process for which the session
        /// create was attempted</param>
        /// <param name="sessionId">The ID of the newly created session</param>
        /// <param name="schedSessId">The ID of the scheduled session which requested
        /// the session creation; zero if it was not from a scheduled session</param>
        /// <param name="errMsg">The error message detailing the reason why the
        /// session create failed</param>
        /// <param name="data">Arbitrary diagnostic data surrounding the create
        /// event. For 'busy' resources, this represents the current status of the
        /// resource. Other specific failures may contain different diagnostic
        /// </param>
        /// <param name="userId">The user that created the operation data.</param>
        public SessionCreateEventArgs(
            SessionCreateState createState = SessionCreateState.Created,
            Guid resourceId = default(Guid),
            Guid processId = default(Guid),
            Guid sessionId = default(Guid),
            Guid userId = default(Guid),
            int schedSessId = 0,
            string errMsg = null,
            IDictionary<Guid, RunnerStatus> data = null,
            object tag = null)
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


        /// <summary>
        /// The state of the create event.
        /// </summary>
        public SessionCreateState State { get; }

        /// <summary>
        /// The ID of the resource on which the new session was created.
        /// </summary>
        public Guid ResourceId { get; }
        

        /// <summary>
        /// The ID of the process which is run as part of the session
        /// </summary>
        public Guid ProcessId { get; }

        /// <summary>
        /// Diagnostic data to aid in investigating why a create session failed.
        /// For 'busy' resources, this may contain the current state of the resource
        /// which was too busy.
        /// </summary>
        public IDictionary<Guid, RunnerStatus> Data { get; }

        /// <summary>
        /// The ID of the scheduled session object which initiated the session - zero if
        /// no such object was referred to.
        /// </summary>
        public int ScheduledSessionId { get; }

        public override bool FromScheduler => ScheduledSessionId > 0;

        /// <summary>
        /// Gets the tag which was used to create this session, or null if none was used
        /// or if it was created by a different connection manager
        /// </summary>
        public object Tag { get; set; }

        public Guid UserId { get; }
    }
}
