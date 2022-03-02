using System;

namespace BluePrism.DigitalWorker.Sessions.Coordination
{
    /// <summary>
    /// The exception that is thrown when a process that is being held while waiting for
    /// other running processes to finish cannot be started within the time allowed. This
    /// might be a background process that was queued while an exclusive process is running
    /// or an exclusive process waiting for a process with a different run mode.
    /// </summary>
    public class DigitalWorkerBusyException : Exception
    {
        public DigitalWorkerBusyException()
        {
        }

        public DigitalWorkerBusyException(string message)
            : base(message)
        {
        }

        public DigitalWorkerBusyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
