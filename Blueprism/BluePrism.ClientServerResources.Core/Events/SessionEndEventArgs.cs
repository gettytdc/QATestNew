using System;

namespace BluePrism.ClientServerResources.Core.Events
{
    /// <summary>
    /// Delegate to handle Session End events
    /// </summary>
    public delegate void SessionEndEventHandler(object sender, SessionEndEventArgs e);

    public class SessionEndEventArgs : BaseResourceEventArgs
    {
        /// <summary>
        /// Creates a new event arguments object.
        /// </summary>
        /// <param name="sessionId">The ID of the started session</param>
        /// <param name="status">The final status of the session</param>
        public SessionEndEventArgs(Guid sessionId, string status)
        {
            SessionId = sessionId;
            UserMessage = status;
        }

        /// <summary>
        /// Gets a string representation of this event.
        /// </summary>
        /// <returns>A string describing this event; actually just a combination of the
        /// session ID and its status</returns>
        public override string ToString() => $"Session:{SessionId}; Status:{UserMessage}";
    }
}
