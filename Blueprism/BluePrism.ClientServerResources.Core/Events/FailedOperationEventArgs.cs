using System;

namespace BluePrism.ClientServerResources.Core.Events
{
    public class FailedCallbackOperationEventArgs
        : EventArgs
    {
        public string Message { get; }
        public string Error { get; }

        public FailedCallbackOperationEventArgs(string msg, string err)
        {
            Message = msg ?? throw new ArgumentNullException(nameof(msg));
            Error = err ?? throw new ArgumentNullException(nameof(err));
        }
    }
    public delegate void InvalidResponseEventHandler(FailedCallbackOperationEventArgs e);
}
