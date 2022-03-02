using System;

namespace BluePrism.Core.Resources
{
    public class SignalResourceConnectionRetryRequiredEventArgs : EventArgs
    {
        public Guid ResourceId { get; }
        public SignalResourceConnectionRetryRequiredEventArgs(Guid resourceId)
        {
            ResourceId = resourceId;
        }
    }
}
